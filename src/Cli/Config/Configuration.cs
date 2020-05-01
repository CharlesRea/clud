using System.Collections.Generic;
using System.Linq;

namespace Clud.Cli.Config
{
    public class Configuration
    {
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;

        public int Port { get; set; }

        public string DockerImage { get; set; }
        public bool SpecifiesDockerImage => !string.IsNullOrEmpty(DockerImage);

        public string Dockerfile { get; set; }
        public string DockerBuildPath { get; set; } // Defaults to the directory containing the Dockerfile
        public bool SpecifiesDockerfile => !string.IsNullOrEmpty(Dockerfile);

        public string Project { get; set; }
        public bool SpecifiesProject => !string.IsNullOrEmpty(Project);

        public List<string> GetValidationErrors()
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
