using System.Collections.Generic;
using System.Linq;
using KubeClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Clud.Api.Infrastructure.DataAccess
{
    public class DataContext : DbContext
    {
        public DbSet<Application> Applications { get; set; }
        public DbSet<Service> Services { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    ApplyEnumToStringValueConverter(property, modelBuilder);
                }
            }
        }

        private static void ApplyEnumToStringValueConverter(IMutableProperty property, ModelBuilder builder)
        {
            if (property.ClrType.IsEnum)
            {
                builder
                    .Entity(property.DeclaringEntityType.ClrType)
                    .Property(property.Name)
                    .HasConversion<string>();
            }
        }
    }

    public class Application
    {
        public int ApplicationId { get; private set; }
        public string Name { get; private set;  }
        public string Namespace { get; private set; }
        public ICollection<Service> Services { get; private set; }

        private Application() { }

        public Application(string name)
        {
            Name = name;
            Namespace = name;
            Services = new List<Service>();
        }

        public void Update(IReadOnlyCollection<ServiceV1> services) // TODO use the DTO passed to the deployment service
        {
            Services = services.Select(s => new Service(s.Metadata.Name)).ToList();
        }
    }

    public class Service
    {
        public int ServiceId { get; private set; }
        public int ApplicationId { get; private set; }
        public string Name { get; private set;  }

        private Service() { }

        public Service(string name)
        {
            Name = name;
        }
    }
}
