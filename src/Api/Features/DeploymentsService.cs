using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clud.Api.Infrastructure.DataAccess;
using Clud.Grpc;
using Grpc.Core;
using KubeClient;
using KubeClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clud.Api.Features
{
    public class DeploymentsService : Deployments.DeploymentsBase
    {
        private readonly DataContext dataContext;
        private readonly KubeApiClient kubeApiClient;
        private readonly CludOptions cludOptions;
        private readonly ILogger<DeploymentsService> logger;

        public DeploymentsService(
            DataContext dataContext,
            KubeApiClient kubeApiClient,
            IOptions<CludOptions> cludOptions,
            ILogger<DeploymentsService> logger
        )
        {
            this.dataContext = dataContext;
            this.kubeApiClient = kubeApiClient;
            this.cludOptions = cludOptions.Value;
            this.logger = logger;
        }

        public override async Task<DeploymentResponse> DeployApplication(DeployCommand command, ServerCallContext context)
        {

            var kubeNamespace = command.Name;

            await CreateNamespace(kubeNamespace);
            var serviceResources  = await DeployServices(command, kubeNamespace);
            var entryPointIngress = await CreateEntryPointIngress(command, kubeNamespace);
            await CleanUpResources(serviceResources, kubeNamespace);

            var application = await dataContext.Applications.SingleOrDefaultAsync(a => a.Name == command.Name);
            if (application != null)
            {
                application.Update(command);
            }
            else
            {
                application = new Application(command);
                dataContext.Applications.Add(application);
                await dataContext.SaveChangesAsync();
            }

            dataContext.Deployments.Add(new Deployment(application, command));
            await dataContext.SaveChangesAsync();

            return new DeploymentResponse
            {
                ManagementUrl = $"https://{cludOptions.BaseHostname}/applications/{command.Name}",
                IngressUrl = entryPointIngress != null ? "https://" + entryPointIngress.Spec.Rules.Single().Host : null,
                Services =
                {
                    serviceResources.Select(service => new DeploymentResponse.Types.Service
                    {
                        Name = service.Service.Metadata.Name,
                        InternalHostname = $"{service.Service.Metadata.Name}.{command.Name}",
                        IngressUrl = service.Ingress != null
                            ? "https://" + service.Ingress.Spec.Rules.Single().Host
                            : null,
                        Ports =
                        {
                            service.Service.Spec.Ports
                                .Where(port => port.Name != KubeNaming.HttpPortName)
                                .Select(port => new DeploymentResponse.Types.Port
                                {
                                    TargetPort = port.TargetPort.Int32Value,
                                    ExposedPort = port.NodePort,
                                    HostName = $"{command.Name}.{cludOptions.BaseHostname}",
                                    Type = port.Protocol,
                                })
                        }
                    })
                }
            };
        }

        private async Task CreateNamespace(string kubeNamespace)
        {
            var namespaceDto = new NamespaceV1
            {
                Metadata = new ObjectMetaV1 { Name = kubeNamespace },
            };
            await kubeApiClient.Dynamic().Apply(namespaceDto, fieldManager: "clud", force: true);
        }

        private async Task<IReadOnlyCollection<ServiceResources>> DeployServices(DeployCommand command, string kubeNamespace)
        {
            var resources = new List<ServiceResources>();
            foreach (var serviceCommand in command.Services)
            {
                resources.Add(await DeployService(serviceCommand, kubeNamespace));
            }

            return resources;
        }

        private async Task<ServiceResources> DeployService(DeployCommand.Types.Service command, string kubeNamespace)
        {
            DeploymentV1 deployment = null;
            StatefulSetV1 statefulSet = null;

            var secret = await CreateSecret();

            if (string.IsNullOrWhiteSpace(command.PersistentStoragePath))
            {
                deployment = await CreateDeployment();
            }
            else
            {
                statefulSet = await CreateStatefulSet();
            }

            var service = await CreateService();
            var ingress = await CreateServiceIngress();

            return new ServiceResources(deployment, statefulSet, service, ingress, secret);

            async Task<SecretV1> CreateSecret()
            {
                if (!command.Secrets.Any())
                {
                    return null;
                }

                var existingSecret = await kubeApiClient.SecretsV1().Get(command.Name, kubeNamespace);
                var secretResource = new SecretV1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = command.Name,
                        Namespace = kubeNamespace,
                    },
                    Type = "Opaque",
                };

                if (existingSecret != null)
                {
                    foreach (var existingData in existingSecret.Data)
                    {
                        secretResource.Data[existingData.Key] = existingData.Value;
                    }
                }

                foreach (var secretCommand in command.Secrets.Where(s => s.Value != null))
                {
                    secretResource.Data[secretCommand.Name] = Convert.ToBase64String(Encoding.UTF8.GetBytes(secretCommand.Value));
                }

                foreach (var existingSecretNames in secretResource.Data.Keys)
                {
                    if (command.Secrets.All(secretCommand => secretCommand.Name != existingSecretNames))
                    {
                        secretResource.Data.Remove(existingSecretNames);
                    }
                }

                return await kubeApiClient.Dynamic().Apply(secretResource, fieldManager: "clud", force: true);
            }

            async Task<DeploymentV1> CreateDeployment()
            {
                var deployment = new DeploymentV1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = command.Name,
                        Namespace = kubeNamespace,
                    },
                    Spec = new DeploymentSpecV1
                    {
                        Selector = new LabelSelectorV1
                        {
                            MatchLabels = { { KubeNaming.AppLabelKey, command.Name } },
                        },
                        Replicas = command.Replicas,
                        Template = new PodTemplateSpecV1
                        {
                            Metadata = new ObjectMetaV1
                            {
                                Name = command.Name,
                                Namespace = kubeNamespace,
                                Labels = { { KubeNaming.AppLabelKey, command.Name } }
                            },
                            Spec = new PodSpecV1
                            {
                                Containers =
                                {
                                    new ContainerV1
                                    {
                                        Name = command.Name,
                                        Image = DockerImageName(),
                                    }
                                },
                            },
                        }
                    }
                };

                AddEnvironmentVariables(deployment.Spec.Template.Spec.Containers.Single().Env);

                return await kubeApiClient.Dynamic().Apply(deployment, fieldManager: "clud", force: true);
            }

            async Task<StatefulSetV1> CreateStatefulSet()
            {
                var statefulSet = new StatefulSetV1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = command.Name,
                        Namespace = kubeNamespace,
                    },
                    Spec = new StatefulSetSpecV1
                    {
                        Selector = new LabelSelectorV1
                        {
                            MatchLabels = { { KubeNaming.AppLabelKey, command.Name } },
                        },
                        Template = new PodTemplateSpecV1
                        {
                            Metadata = new ObjectMetaV1
                            {
                                Name = command.Name,
                                Namespace = kubeNamespace,
                                Labels = { { KubeNaming.AppLabelKey, command.Name } }
                            },
                            Spec = new PodSpecV1
                            {
                                Containers =
                                {
                                    new ContainerV1
                                    {
                                        Name = command.Name,
                                        Image = DockerImageName(),
                                        VolumeMounts = { new VolumeMountV1
                                        {
                                            Name = command.Name,
                                            MountPath = command.PersistentStoragePath,
                                        }}
                                    },
                                },
                            },
                        },
                        VolumeClaimTemplates =
                        {
                            new PersistentVolumeClaimV1
                            {
                                Metadata = new ObjectMetaV1
                                {
                                    Name = command.Name,
                                    Namespace = kubeNamespace,
                                },
                                Spec = new PersistentVolumeClaimSpecV1
                                {
                                    AccessModes = { "ReadWriteOnce" },
                                    Resources = new ResourceRequirementsV1
                                    {
                                        Requests = {{ "storage", "100Mi" }},
                                    }
                                }
                            }
                        }
                    }
                };

                AddEnvironmentVariables(statefulSet.Spec.Template.Spec.Containers.Single().Env);

                return await kubeApiClient.Dynamic().Apply(statefulSet, fieldManager: "clud", force: true);
            }

            string DockerImageName()
            {
                return command.IsPublicDockerImage
                    ? command.DockerImage
                    : $"{KubeNaming.DockerRegistryLocation}/{command.DockerImage}";
            }

            void AddEnvironmentVariables(List<EnvVarV1> envVarV1s)
            {
                envVarV1s.AddRange(command.EnvironmentVariables.Select(env => new EnvVarV1
                {
                    Name = env.Name,
                    Value = env.Value
                }));

                envVarV1s.AddRange(command.Secrets.Select(secret => new EnvVarV1
                {
                    Name = secret.Name,
                    ValueFrom = new EnvVarSourceV1
                    {
                        SecretKeyRef = new SecretKeySelectorV1
                        {
                            Name = command.Name,
                            Key = secret.Name,
                            Optional = false,
                        }
                    }
                }));
            }


            async Task<ServiceV1> CreateService()
            {
                var service = new ServiceV1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = command.Name,
                        Namespace = kubeNamespace,
                    },
                    Spec = new ServiceSpecV1
                    {
                        Selector = { { KubeNaming.AppLabelKey, command.Name } },
                    },
                };

                if (command.HttpPort != null)
                {
                    service.Spec.Ports.Add(new ServicePortV1
                    {
                        Name = KubeNaming.HttpPortName,
                        Protocol = "TCP",
                        Port = command.HttpPort.Value
                    });
                }

                service.Spec.Ports.AddRange(command.TcpPorts.Select(port => new ServicePortV1
                {
                    Name = $"tcp-{port}",
                    Protocol = "TCP",
                    Port = port,
                }));

                service.Spec.Ports.AddRange(command.UdpPorts.Select(port => new ServicePortV1
                {
                    Name = $"udp-{port}",
                    Protocol = "UDP",
                    Port = port,
                }));

                return await kubeApiClient.Dynamic().Apply(service, fieldManager: "clud", force: true);
            }

            async Task<IngressV1Beta1> CreateServiceIngress()
            {
                if (command.HttpPort == null)
                {
                    return null;
                }

                var ingress = new IngressV1Beta1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = command.Name,
                        Namespace = kubeNamespace,
                    },
                    Spec = new IngressSpecV1Beta1
                    {
                        Rules =
                        {
                            new IngressRuleV1Beta1
                            {
                                Host = $"{command.Name}-{kubeNamespace}.{cludOptions.BaseHostname}",
                                Http = new HTTPIngressRuleValueV1Beta1
                                {
                                    Paths =
                                    {
                                        new HTTPIngressPathV1Beta1
                                        {
                                            Path = "/",
                                            Backend = new IngressBackendV1Beta1
                                            {
                                                ServiceName = service.Metadata.Name,
                                                ServicePort = KubeNaming.HttpPortName,
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    },
                };

                return await kubeApiClient.Dynamic().Apply(ingress, fieldManager: "clud", force: true);
            }
        }

        private class ServiceResources
        {
            public DeploymentV1 Deployment { get; }
            public StatefulSetV1 StatefulSet { get; }
            public ServiceV1 Service { get; }
            public IngressV1Beta1 Ingress { get; }
            public SecretV1 Secret { get; }

            public ServiceResources(DeploymentV1 deployment, StatefulSetV1 statefulSet, ServiceV1 service, IngressV1Beta1 ingress, SecretV1 secret)
            {
                Deployment = deployment;
                StatefulSet = statefulSet;
                Service = service;
                Ingress = ingress;
                Secret = secret;
            }
        }

        private async Task<IngressV1Beta1> CreateEntryPointIngress(DeployCommand deployCommand, string kubeNamespace)
        {
            if (string.IsNullOrWhiteSpace(deployCommand.EntryPoint))
            {
                await DeleteExistingIngress();
                return null;
            }
            else
            {
                return await CreateIngress();
            }

            async Task DeleteExistingIngress()
            {
                await kubeApiClient.IngressesV1Beta1().Delete(KubeNaming.EntryPointIngressName, kubeNamespace);
            }

            async Task<IngressV1Beta1> CreateIngress()
            {
                var ingress = new IngressV1Beta1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = KubeNaming.EntryPointIngressName,
                        Namespace = kubeNamespace,
                    },
                    Spec = new IngressSpecV1Beta1
                    {
                        Rules =
                        {
                            new IngressRuleV1Beta1
                            {
                                Host = $"{deployCommand.Name}.{cludOptions.BaseHostname}",
                                Http = new HTTPIngressRuleValueV1Beta1
                                {
                                    Paths =
                                    {
                                        new HTTPIngressPathV1Beta1
                                        {
                                            Path = "/",
                                            Backend = new IngressBackendV1Beta1
                                            {
                                                ServiceName = deployCommand.EntryPoint,
                                                ServicePort = KubeNaming.HttpPortName,
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    },
                };

                return await kubeApiClient.Dynamic().Apply(ingress, fieldManager: "clud", force: true);
            }
        }

        private async Task CleanUpResources(IReadOnlyCollection<ServiceResources> expectedResources, string kubeNamespace)
        {
            await CleanupServices();
            await CleanupDeployments();
            await CleanupStatefulSets();
            await CleanupIngresses();
            await CleanupSecrets();

            async Task CleanupServices()
            {
                var allServices = await kubeApiClient.ServicesV1().List(kubeNamespace: kubeNamespace);
                foreach (var service in allServices.Items.Where(service =>
                    expectedResources.All(r => r.Service.Metadata.Name != service.Metadata.Name)))
                {
                    await kubeApiClient.ServicesV1().Delete(service.Metadata.Name, kubeNamespace);
                }
            }

            async Task CleanupDeployments()
            {
                var allDeployments = await kubeApiClient.DeploymentsV1().List(kubeNamespace: kubeNamespace);
                foreach (var deployment in allDeployments.Items.Where(deployment =>
                    expectedResources.All(r => r.Deployment?.Metadata.Name != deployment.Metadata.Name)))
                {
                    await kubeApiClient.DeploymentsV1().Delete(deployment.Metadata.Name, kubeNamespace);
                }
            }

            async Task CleanupIngresses()
            {
                var allIngresses = await kubeApiClient.IngressesV1Beta1().List(kubeNamespace: kubeNamespace);
                foreach (var ingress in allIngresses.Items.Where(ingress =>
                    expectedResources.All(r => r.Ingress?.Metadata.Name != ingress.Metadata.Name) && ingress.Metadata.Name != KubeNaming.EntryPointIngressName))
                {
                    await kubeApiClient.IngressesV1Beta1().Delete(ingress.Metadata.Name, kubeNamespace);
                }
            }

            async Task CleanupStatefulSets()
            {
                var allStatefulSets = await kubeApiClient.StatefulSetV1().List(kubeNamespace: kubeNamespace);
                foreach (var statefulSet in allStatefulSets.Items.Where(statefulSet =>
                    expectedResources.All(r => r.StatefulSet?.Metadata.Name != statefulSet.Metadata.Name)))
                {
                    await kubeApiClient.StatefulSetV1().Delete(statefulSet.Metadata.Name, kubeNamespace);
                }
            }

            async Task CleanupSecrets()
            {
                var allSecrets = await kubeApiClient.SecretsV1().List(kubeNamespace: kubeNamespace);
                foreach (var secret in allSecrets.Items.Where(secret =>
                    expectedResources.All(r => r.Secret?.Metadata.Name != secret.Metadata.Name)))
                {
                    await kubeApiClient.SecretsV1().Delete(secret.Metadata.Name, kubeNamespace);
                }
            }
        }
    }
}
