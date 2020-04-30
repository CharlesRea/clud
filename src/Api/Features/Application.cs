using System;
using System.Collections.Generic;
using Clud.Grpc;

namespace Clud.Api.Features
{
    public class Application
    {
        public int ApplicationId { get; private set; }
        public string Name { get; private set;  }
        public string Description { get; private set;  }
        public string Owner { get; private set;  }
        public string Repository { get; private set;  }
        public DateTimeOffset UpdatedDateTime { get; private set; }
        public ICollection<Service> Services { get; private set; }

        public string Namespace => Name;

        private Application() { }

        public Application(CreateDeploymentRequest deployment)
        {
            Name = deployment.Name;
            Description = deployment.Description;
            Owner = deployment.Owner;
            Repository = deployment.Repository;
            UpdatedDateTime = DateTimeOffset.UtcNow;

            Services = new List<Service>
            {
                new Service(deployment.Name),
            };
        }

        public void Update(CreateDeploymentRequest deployment)
        {
            Name = deployment.Name;
            Description = deployment.Description;
            Owner = deployment.Owner;
            Repository = deployment.Repository;
            UpdatedDateTime = DateTimeOffset.UtcNow;

            // TODO handle service updating
        }
    }
}
