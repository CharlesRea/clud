using System.Linq;
using System.Threading.Tasks;
using Clud.Api.Infrastructure.DataAccess;
using Clud.Grpc;
using Grpc.Core;
using KubeClient;
using KubeClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clud.Api.Services
{
    public class DeploymentsService : Deployments.DeploymentsBase
    {
        private readonly DataContext dataContext;
        private readonly KubeApiClient kubeApiClient;
        private readonly ILogger<DeploymentsService> logger;

        private const string DockerImageRegistryLocation = "localhost:5000";

        public DeploymentsService(DataContext dataContext, KubeApiClient kubeApiClient, ILogger<DeploymentsService> logger)
        {
            this.dataContext = dataContext;
            this.kubeApiClient = kubeApiClient;
            this.logger = logger;
        }

        public override async Task<CreateDeploymentResponse> CreateDeployment(CreateDeploymentRequest request, ServerCallContext context)
        {
            logger.LogInformation($"Received request {request.Name} / {request.DockerImage} / {request.Port}");

            var kubeNamespace = request.Name;

            var existingNamespace = await kubeApiClient.NamespacesV1().Get(request.Name);

            if (existingNamespace == null)
            {
                await kubeApiClient.NamespacesV1().Create(new NamespaceV1
                {
                    Metadata = new ObjectMetaV1 { Name = kubeNamespace }
                });
            }

            var deployment = new DeploymentV1
            {
                Metadata = new ObjectMetaV1
                {
                    Name = request.Name,
                    Namespace = kubeNamespace,
                },
                Spec = new DeploymentSpecV1
                {
                    Selector = new LabelSelectorV1
                    {
                        MatchLabels = {{ "App", request.Name }},
                    },
                    Template = new PodTemplateSpecV1
                    {
                        Metadata = new ObjectMetaV1
                        {
                            Name = request.Name,
                            Namespace = kubeNamespace,
                            Labels = {{ "App", request.Name }}
                        },
                        Spec = new PodSpecV1
                        {
                            Containers =
                            {
                                new ContainerV1
                                {
                                    Name = request.Name,
                                    Image = request.IsPublicDockerImage
                                        ? request.DockerImage
                                        : $"{DockerImageRegistryLocation}/{request.DockerImage}",
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
                    Name = request.Name,
                    Namespace = kubeNamespace,
                },
                Spec = new ServiceSpecV1
                {
                    Selector = {{ "App", deployment.Spec.Template.Metadata.Labels["App"] }},
                    Ports =
                    {
                        // TODO - Allow multiple port bindings
                        new ServicePortV1
                        {
                            Name = "default",
                            Protocol = "TCP",
                            Port = request.Port, // This is the inbound port on the Service
                            TargetPort = request.Port, // This is the inbound port on the Pod (i.e. the underlying application)
                    }
                    }
                },
            };

            await kubeApiClient.Dynamic().Apply(service, fieldManager: "clud", force: true);

            var ingress = new IngressV1Beta1
            {
                Metadata = new ObjectMetaV1
                {
                    Name = request.Name,
                    Namespace = kubeNamespace,
                },
                Spec = new IngressSpecV1Beta1
                {
                    Rules =
                    {
                        new IngressRuleV1Beta1
                        {
                            Host = $"{request.Name}.clud",
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
                    // fine since apparently Traefik just registeres the Traefik-UI ingress rule to apply to every
                    // endpoint, and since the cert is a wildcard cert this continues to be accessible over HTTPS...
                    // Tls =
                    // {
                    //     new IngressTLSV1Beta1 { SecretName = "traefik-tls-cert" },
                    // }
                },
            };

            // TODO clean up no longer existing services

            await kubeApiClient.Dynamic().Apply(ingress, fieldManager: "clud", force: true);

            var application = await dataContext.Applications.SingleOrDefaultAsync(a => a.Name == request.Name);
            if (application == null)
            {
                application = new Application(request.Name);
                dataContext.Applications.Add(application);
            }

            application.Update(new[] { service });
            await dataContext.SaveChangesAsync();


            return new CreateDeploymentResponse();
        }
    }
}
