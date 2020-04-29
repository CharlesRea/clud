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
        private readonly ILogger<DeploymentsService> logger;
        private readonly ILoggerFactory loggerFactory;

        public DeploymentsService(DataContext dataContext, ILogger<DeploymentsService> logger, ILoggerFactory loggerFactory)
        {
            this.dataContext = dataContext;
            this.logger = logger;
            this.loggerFactory = loggerFactory;
        }

        public override async Task<CreateDeploymentResponse> CreateDeployment(CreateDeploymentRequest request, ServerCallContext context)
        {
            logger.LogInformation($"Received request {request.Name} / {request.DockerImage} / {request.Port}");

            const string kubeNamespace = "default";

            var kubeClientOptions = K8sConfig.Load().ToKubeClientOptions();
            kubeClientOptions.KubeNamespace = kubeNamespace;
            kubeClientOptions.LoggerFactory = loggerFactory;
            var client = KubeApiClient.Create(kubeClientOptions);

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
                                    Image = $"localhost:5000/{request.DockerImage}",
                                }
                            }
                        }
                    }
                }
            };

            await client.Dynamic().Apply(deployment, fieldManager: "clud", force: true);

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

            await client.Dynamic().Apply(service, fieldManager: "clud", force: true);

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

            await client.Dynamic().Apply(ingress, fieldManager: "clud", force: true);

            var existingApplication = await dataContext.Applications.SingleOrDefaultAsync(a => a.Name == request.Name);
            if (existingApplication == null)
            {
                dataContext.Applications.Add(new Application(request.Name));
                await dataContext.SaveChangesAsync();
            }

            return new CreateDeploymentResponse();
        }
    }
}
