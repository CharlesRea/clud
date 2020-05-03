using Clud.Grpc;

namespace Clud.Api.Features
{
    public class Service
    {
        public int ServiceId { get; private set; }
        public int ApplicationId { get; private set; }
        public string Name { get; private set;  }

        private Service() { }

        public Service(CreateDeploymentRequest.Types.ServiceDeploymentDetails service)
        {
            Name = service.ServiceName;
        }

        public void Update(CreateDeploymentRequest.Types.ServiceDeploymentDetails service)
        {
            Name = service.ServiceName;
        }
    }
}
