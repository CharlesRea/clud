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

            Services = deployment.Services.Select(s => new Service(s)).ToList();
        }

        public (IReadOnlyCollection<Service> newServices, IReadOnlyCollection<Service> deletedServices) Update(CreateDeploymentRequest deployment)
        {
            Name = deployment.Name;
            Description = deployment.Description;
            Owner = deployment.Owner;
            Repository = deployment.Repository;
            UpdatedDateTime = DateTimeOffset.UtcNow;

            var updatedServices = UpdateServices();
            return updatedServices;

            (IReadOnlyCollection<Service> newServices, IReadOnlyCollection<Service> deletedServices) UpdateServices()
            {
                var newServices = new List<Service>();
                if (Services == null)
                {
                    throw new NavigationPropertyNotLoadedException(nameof(Services));
                }

                var existingServices = Services.ToDictionary(s => s.Name);

                foreach (var service in deployment.Services)
                {
                    var existingService = existingServices.GetValueOrDefault(service.ServiceName);
                    if (existingService != null)
                    {
                        existingService.Update(service);
                    }
                    else
                    {
                        var newService = new Service(service);
                        Services.Add(newService);
                        newServices.Add(newService);
                    }
                }

                var deletedServices = Services.Where(service => deployment.Services.All(s => service.Name != s.ServiceName)).ToList();
                foreach (var service in deletedServices)
                {
                    Services.Remove(service);
                }

                return (newServices, deletedServices);
            }
        }
    }
}
