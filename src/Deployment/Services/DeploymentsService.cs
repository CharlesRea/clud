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
            var kubeClientOptions = K8sConfig.Load().ToKubeClientOptions();
            kubeClientOptions.KubeNamespace = "default";
            kubeClientOptions.LoggerFactory = loggerFactory;
            var client = KubeApiClient.Create(kubeClientOptions);

            var pod = new PodV1
            {
                Metadata = new ObjectMetaV1
                {
                    Name = request.Name,
                    Namespace = "default",
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
            };

            await client.Dynamic().Apply(pod, fieldManager: "clud", force: true);

            return new CreateDeploymentResponse();
        }
    }
}
