using Clud.Api.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Clud.Api.Infrastructure.DataAccess
{
    public class DataContext : DbContext
    {
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationHistory> ApplicationHistories { get; set; }
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
}
