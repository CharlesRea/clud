using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clud.Cli.Helpers;
using Clud.Cli.Templates;
using FluentValidation;
using SharpYaml;
using SharpYaml.Serialization;

namespace Clud.Cli.Config
{
    public class ApplicationConfiguration
    {
        public string Name { get; set; }

        public string Owner { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;

        public string EntryPoint { get; set; }

        public IList<ServiceConfiguration> Services { get; set; }

        public class Validator : AbstractValidator<ApplicationConfiguration>
        {
            public Validator()
            {
                RuleFor(c => c.Name)
                    .NotEmpty().WithMessage("The 'name' field must be set")
                    .ValidResourceName();

                RuleFor(c => c.EntryPoint)
                    .NotEmpty()
                    .WithMessage("No 'entryPoint' has been set. The application will not be accessible publicly. The 'entryPoint' can be set to the name of a service")
                    .WithSeverity(Severity.Warning);

                RuleFor(c => c.Services)
                    .NotEmpty().WithMessage("You must specify one or more services");

                RuleForEach(c => c.Services)
                    .SetValidator(new ServiceConfiguration.Validator());
            }
        }

        public static ParseResult<ApplicationConfiguration> Parse(TextReader yamlStream)
        {
            var yamlSerializer = new Serializer(new SerializerSettings
            {
                NamingConvention = new CamelCaseNamingConvention(),
                EmitTags = false
            });

            try
            {
                var configuration = yamlSerializer.Deserialize<ApplicationConfiguration>(yamlStream);

                var validator = new Validator();
                var validationResult = validator.Validate(configuration);
                var parseResult = new ParseResult<ApplicationConfiguration>(configuration, validationResult);

                foreach (var service in configuration.Services)
                {
                    parseResult.AddWarningsAndErrors(service.ParseTemplateOptions(yamlSerializer));
                }

                return parseResult;
            }
            catch (YamlException e)
            {
               throw new ConfigParseException(e);
            }
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

        [YamlMember("template")]
        public string TemplateName { get; set; }
        public bool SpecifiesTemplate => !string.IsNullOrEmpty(TemplateName);
        [YamlMember("templateOptions")]
        public Dictionary<string, object> RawTemplateOptions { get; set; }
        [YamlIgnore]
        public TemplateOptions TemplateOptions { get; private set; }

        public int? HttpPort { get; set; }
        public IList<int> TcpPorts { get; set; } = new List<int>();
        public IList<int> UdpPorts { get; set; } = new List<int>();
        public IReadOnlyCollection<int> Ports =>
            TcpPorts.Concat(UdpPorts).Concat(HttpPort != null ? new[] { HttpPort.Value } : new int[0]).ToList();

        public int Replicas { get; set; } = 1;

        public string PersistentStoragePath { get; set; }

        public IDictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        public IList<string> Secrets { get; set; } = new List<string>();

        public class Validator : AbstractValidator<ServiceConfiguration>
        {
            public Validator()
            {
                RuleFor(c => c.Name)
                    .NotEmpty().WithMessage("The 'name' field must be set")
                    .ValidResourceName();

                RuleFor(c => c)
                    .Must(c => new[] { c.DockerImage, c.Dockerfile, c.TemplateName }.Count(value => !string.IsNullOrEmpty(value)) == 1)
                    .WithMessage(c => $"Service {c.Name}: You must specify either a dockerImage, a dockerfile, or a template");

                RuleFor(c => c.DockerBuildPath)
                    .Empty()
                    .Unless(c => c.SpecifiesDockerfile)
                    .WithMessage(c => $"Service {c.Name}: You should only specify a dockerBuildPath when also specifying a dockerfile");

                RuleFor(c => c.Ports)
                    .NotEmpty()
                    .WithMessage(c => $"Service {c.Name} does not have any ports specified. It will not be publicly accessible")
                    .WithSeverity(Severity.Warning);

                RuleFor(c => c.Ports)
                    .Must(ports => ports.Distinct().Count() == ports.Count())
                    .WithMessage(c => $"Service {c.Name}: ports must be unique");

                RuleFor(c => c.Replicas)
                    .Equal(1)
                    .When(c => !string.IsNullOrWhiteSpace(c.PersistentStoragePath))
                    .WithMessage(c => $"Service {c.Name}: cannot specify both replicas and a persistentStoragePath");
            }
        }

        public ParseResult<TemplateOptions> ParseTemplateOptions(Serializer serializer)
        {
            if (TemplateName == null)
            {
                return ParseResult<TemplateOptions>.Success(null);
            }

            if (!Template.IsValidTemplateName(TemplateName))
            {
                return ParseResult<TemplateOptions>.Failed($"Service {Name}: Unrecognised template {TemplateName}. Valid options are {string.Join(", ", Template.TemplateNames)}");
            }

            if (RawTemplateOptions == null)
            {
                return ParseResult<TemplateOptions>.Failed($"Service {Name}: No template options provided");
            }

            var template = Template.GetTemplate(TemplateName);

            var serialisedOptions = serializer.Serialize(RawTemplateOptions);
            TemplateOptions = (TemplateOptions) serializer.Deserialize(serialisedOptions, template.OptionsType);

            var validationResult = template.Validate(TemplateOptions);

            return new ParseResult<TemplateOptions>(TemplateOptions, validationResult);
        }
    }
}
