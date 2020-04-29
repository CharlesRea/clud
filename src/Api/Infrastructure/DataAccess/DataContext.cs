using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Clud.Api.Infrastructure.DataAccess
{
    public class DataContext : DbContext
    {
        public DbSet<Application> Applications { get; set; }

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

        private Application() { }

        public Application(string name)
        {
            Name = name;
        }
    }
}
