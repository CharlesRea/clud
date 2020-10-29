using System.IO;
using Clud.Cli.Config;
using FluentValidation;

namespace Clud.Cli.Templates
{
    public class AspNetCoreTemplate : Template<AspNetCoreTemplate.Options>
    {
        public override string TemplateName => "aspnetcore";

        protected override IValidator<Options> OptionsValidator => new Options.Validator();

        public override string GenerateDockerfile(ServiceConfiguration service, Options options, string configFileDirectory)
        {
            var projectName = GetProjectName();

            return $@"
FROM {options.BuildImage} AS build-env
WORKDIR /app

{(ShouldInstallYarn() ? @"
# Install Yarn
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash
RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
RUN echo ""deb https://dl.yarnpkg.com/debian/ stable main"" | tee /etc/apt/sources.list.d/yarn.list
            RUN apt-get update && apt-get install -y nodejs yarn
" : "")}

COPY . ./

WORKDIR {options.ProjectDirectory}
RUN dotnet publish {projectName}.csproj -c {options.Configuration} -o /app/out

# Build runtime image
FROM {options.RuntimeImage}
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [""dotnet"", ""{projectName}.dll""]";

            string GetProjectName()
            {
                var name = options.ProjectName;

                if (name == null)
                {
                    var projectDirectory = Path.Join(configFileDirectory, options.ProjectDirectory);
                    var csprojFiles = Directory.GetFiles(projectDirectory, "*.csproj");
                    if (csprojFiles.Length == 1)
                    {
                        name = Path.GetFileNameWithoutExtension(csprojFiles[0]);
                    }
                    else if (csprojFiles.Length == 0)
                    {
                        throw new ValidationException(
                            $"Could not automatically determine the C# project name for service {service.Name} (we tried " +
                            $"looking for a single csproj file in the projectDirectory ({projectDirectory}), but " +
                            $"found {csprojFiles.Length} possible candidates. Please ensure your projectDirectory setting " +
                            $"is correct, or explicitly specify a projectName option");
                    }
                }

                return name;
            }

            bool ShouldInstallYarn()
            {
                if (options.RequiresYarn)
                {
                    return true;
                }
                else
                {
                    var projectFilePath = Path.Join(configFileDirectory, options.ProjectDirectory, $"{projectName}.csproj");
                    var projectFileContents = File.ReadAllText(projectFilePath);
                    return projectFileContents.Contains("yarn");
                }
            }
        }

        public class Options : TemplateOptions
        {
            public string DotNetVersion { get; set; } = "3.1";

            public string ProjectDirectory { get; set; }
            public string ProjectName { get; set; }
            public string Configuration { get; set; } = "Release";

            public string BuildImage => $"mcr.microsoft.com/dotnet/core/sdk:{DotNetVersion}";
            public string RuntimeImage => $"mcr.microsoft.com/dotnet/core/aspnet:{DotNetVersion}";

            public bool RequiresYarn { get; set; } = false;

            public class Validator : AbstractValidator<Options>
            {
                public Validator()
                {
                    RuleFor(o => o.ProjectDirectory).NotEmpty().WithMessage("projectDirectory template option is required for the aspnetcore template");
                    RuleFor(o => o.ProjectDirectory).Must(dir => !Path.IsPathRooted(dir)).WithMessage(
                        "projectDirectory template option must be a relative path, relative to the clud configuration file's directory");
                }
            }

            public override string DockerBuildPath => "."; // Relative to the config file path
        }
    }
}
