using System.Collections.Generic;
using System.Linq;

namespace Clud.Cli.Config
{
    public class Configuration
    {
        public string Name { get; set; }

        public string Owner { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;

        public string EntryPoint { get; set; }

        public ICollection<ServiceConfiguration> Services { get; set; }

        public IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Name))
            {
                errors.Add("You must specify a name");
            }

            if (string.IsNullOrEmpty(EntryPoint))
            {
                errors.Add("You must specify an entryPoint");
            }

            if (!Services.Any())
            {
                errors.Add("You must specify one or more services");
            }

            var serviceNames = Services.Select(service => service.Name).ToList();
            if (!serviceNames.Contains(EntryPoint))
            {
                errors.Add($"Invalid entryPoint '{EntryPoint}' (valid values are {string.Join(", ", serviceNames)})");
            }

            errors.AddRange(Services.SelectMany((service, index) =>
                service.GetValidationErrors().Select(error => $"services[{index}]: {error}").ToList()
            ));

            return errors;
        }
    }

    public class ServiceConfiguration
    {
        public string Name { get; set; }

        public string DockerImage { get; set; }
        public bool SpecifiesDockerImage => !string.IsNullOrEmpty(DockerImage);

        public string Dockerfile { get; set; }
        public string DockerBuildPath { get; set; } // Defaults to the directory containing the Dockerfile
        public bool SpecifiesDockerfile => !string.IsNullOrEmpty(Dockerfile);

        public string Project { get; set; }
        public bool SpecifiesProject => !string.IsNullOrEmpty(Project);

        public int Port { get; set; }

        public IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Name))
            {
                errors.Add("You must specify a name");
            }

            if (new[] { DockerImage, Dockerfile, Project }.Count(value => !string.IsNullOrEmpty(value)) != 1)
            {
                errors.Add("You must specify either a dockerImage, a dockerfile, or a project");
            }

            if (!string.IsNullOrEmpty(DockerBuildPath) && string.IsNullOrEmpty(Dockerfile))
            {
                errors.Add("You should only specify a dockerBuildPath when also specifying a dockerfile");
            }

            return errors;
        }
    }
}
