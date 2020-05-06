using System.Linq;
using System.Threading.Tasks;
using Clud.Api.Infrastructure.DataAccess;
using Clud.Grpc;
using Grpc.Core;
using KubeClient;
using KubeClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

namespace Clud.Api.Features
{
    public class DeploymentsService : Deployments.DeploymentsBase
    {
        private readonly DataContext dataContext;
        private readonly KubeApiClient kubeApiClient;
        private readonly CludOptions cludOptions;
        private readonly ILogger<DeploymentsService> logger;

        private const string DockerRegistryLocation = "localhost:5000";

        public DeploymentsService(DataContext dataContext, KubeApiClient kubeApiClient, IOptions<CludOptions> cludOptions, ILogger<DeploymentsService> logger)
        {
            this.dataContext = dataContext;
            this.kubeApiClient = kubeApiClient;
            this.cludOptions = cludOptions.Value;
            this.logger = logger;
        }

        public override async Task<CreateDeploymentResponse> CreateDeployment(CreateDeploymentRequest request, ServerCallContext context)
        {
            var kubeNamespace = request.Name;

            var namespaceDto = new NamespaceV1
            {
                Metadata = new ObjectMetaV1 { Name = kubeNamespace },
            };
            await kubeApiClient.Dynamic().Apply(namespaceDto, fieldManager: "clud", force: true);

            foreach (var serviceRequest in request.Services)
            {
                var serviceName = serviceRequest.ServiceName;

                var deployment = new DeploymentV1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = serviceName,
                        Namespace = kubeNamespace,
                    },
                    Spec = new DeploymentSpecV1
                    {
                        Selector = new LabelSelectorV1
                        {
                            MatchLabels = {{ KubeNaming.AppLabelKey, serviceName }},
                        },
                        Template = new PodTemplateSpecV1
                        {
                            Metadata = new ObjectMetaV1
                            {
                                Name = serviceName,
                                Namespace = kubeNamespace,
                                Labels = {{ KubeNaming.AppLabelKey, serviceName }}
                            },
                            Spec = new PodSpecV1
                            {
                                Containers =
                                {
                                    new ContainerV1
                                    {
                                        Name = serviceName,
                                        Image = serviceRequest.IsPublicDockerImage
                                            ? serviceRequest.DockerImage
                                            : $"{DockerRegistryLocation}/{serviceRequest.DockerImage}",
                                    }
                                }
                            }
                        }
                    }
                };

                await kubeApiClient.Dynamic().Apply(deployment, fieldManager: "clud", force: true);

                var service = new ServiceV1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = serviceName,
                        Namespace = kubeNamespace,
                    },
                    Spec = new ServiceSpecV1
                    {
                        Selector = {{ KubeNaming.AppLabelKey, deployment.Spec.Template.Metadata.Labels["App"] }},
                        Ports =
                        {
                            // TODO - Allow multiple port bindings
                            new ServicePortV1
                            {
                                Name = KubeNaming.ServiceDefaultPortName,
                                Protocol = "TCP",
                                Port = serviceRequest.Port, // This is the inbound port on the Service
                                TargetPort = serviceRequest.Port, // This is the inbound port on the Pod (i.e. the underlying application)
                            }
                        }
                    },
                };

                await kubeApiClient.Dynamic().Apply(service, fieldManager: "clud", force: true);

                // We only create an ingress rule for the entry point - all other services are only accessed from within the cluster
                if (serviceName != request.EntryPoint) continue;

                var ingress = new IngressV1Beta1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = serviceName,
                        Namespace = kubeNamespace,
                    },
                    Spec = new IngressSpecV1Beta1
                    {
                        Rules =
                        {
                            new IngressRuleV1Beta1
                            {
                                Host = $"{request.Name}.{cludOptions.BaseHostname}",
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
                                                ServicePort = service.Spec.Ports.Single().Name,
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        // TODO Traefik doesn't pick up the Ingress rule if the TLS entry is defined. However it works
                        // fine since apparently Traefik just registers the Traefik-UI ingress rule to apply to every
                        // endpoint, and since the cert is a wildcard cert this continues to be accessible over HTTPS...
                        // Tls =
                        // {
                        //     new IngressTLSV1Beta1 { SecretName = "traefik-tls-cert" },
                        // }
                    },
                };

                await kubeApiClient.Dynamic().Apply(ingress, fieldManager: "clud", force: true);
            }

            var application = await dataContext.Applications
                .Include(a => a.Services)
                .SingleOrDefaultAsync(a => a.Name == request.Name);
            var applicationExists = application != null;

            if (applicationExists)
            {
                var (newServices, deletedServices) = application.Update(request);

                foreach (var deletedService in deletedServices)
                {
                    await kubeApiClient.ServicesV1().Delete(deletedService.Name, kubeNamespace);
                }

                var historyMessage =
                    $"Update deployed"
                      + (newServices.Any() ? $". New services added: {string.Join(", ", newServices.Select(s => s.Name))}" : "")
                      + (deletedServices.Any() ? $". Services removed: {string.Join(", ", deletedServices.Select(s => s.Name))}" : "");

                dataContext.ApplicationHistories.Add(new ApplicationHistory(application, historyMessage));
                await dataContext.SaveChangesAsync();
            }

            if (!applicationExists)
            {
                application = new Application(request);
                dataContext.Applications.Add(application);
                await dataContext.SaveChangesAsync();
                dataContext.ApplicationHistories.Add(new ApplicationHistory(
                    application,
                    $"Application created, with services {string.Join(", ", application.Services.Select(s => s.Name))}"
                ));
                await dataContext.SaveChangesAsync();
            }

            return new CreateDeploymentResponse();
            // TODO return a URL to the CLI
            // - URL of the app?
            // - clud URL to see status?
        }
    }
}
