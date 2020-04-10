using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using k8s;
using Microsoft.Extensions.Logging;

namespace Clud.Deployment.Services
{
    public class PodsService : Pods.PodsBase
    {
        private readonly ILogger<PodsService> logger;

        public PodsService(ILogger<PodsService> logger)
        {
            this.logger = logger;
        }

        public override Task<PodsReply> ListPods(PodsRequest request, ServerCallContext context)
        {
            logger.LogInformation("Starting request");

            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            var client = new Kubernetes(config);

            var pods = client.ListNamespacedPod("default");

            var podsReply = new PodsReply();
            podsReply.Pods.AddRange(pods.Items.Select(pod => new PodsReply.Types.Pod { Name = pod.Metadata.Name }));

            return Task.FromResult(podsReply);
        }
    }
}
