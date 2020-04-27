using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using KubeClient;
using KubeClient.Models;
using Microsoft.Extensions.Logging;

namespace Clud.Deployment.Services
{
    public class DeploymentsService : Deployments.DeploymentsBase
    {
        private readonly ILogger<PodsService> logger;
        private readonly ILoggerFactory loggerFactory;

        public DeploymentsService(ILogger<PodsService> logger, ILoggerFactory loggerFactory)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
        }

        public override async Task<CreateDeploymentResponse> CreateDeployment(CreateDeploymentRequest request, ServerCallContext context)
        {
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
                                    Name = "helloworld",
                                    Image = "zerokoll/helloworld",
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
                        new ServicePortV1 { Name = "web", Port = 80, TargetPort = 80, Protocol = "TCP", }
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

            return new CreateDeploymentResponse();
        }
    }
}
