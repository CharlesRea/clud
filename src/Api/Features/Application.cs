using System;
using System.Collections.Generic;
using System.Linq;
using Clud.Api.Infrastructure.DataAccess;
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

        public string Namespace => Name;

        private Application() { }

        public Application(DeployCommand deployment)
        {
            Name = deployment.Name;
            Description = deployment.Description;
            Owner = deployment.Owner;
            Repository = deployment.Repository;
            UpdatedDateTime = DateTimeOffset.UtcNow;
        }

        public void Update(DeployCommand deployment)
        {
            Name = deployment.Name;
            Description = deployment.Description;
            Owner = deployment.Owner;
            Repository = deployment.Repository;
            UpdatedDateTime = DateTimeOffset.UtcNow;
        }
    }
}
