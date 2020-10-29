using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clud.Cli.Config;
using FluentValidation;
using FluentValidation.Results;

namespace Clud.Cli.Templates
{
    public abstract class Template
    {
        public abstract string TemplateName { get; }
        public abstract Type OptionsType { get; }
        public abstract ValidationResult Validate(TemplateOptions options);
        public abstract string GenerateDockerfile(ServiceConfiguration service, string configFileDirectory);

        private static readonly IReadOnlyDictionary<string, Template> Templates;

        static Template()
        {
            var templateClasses = Assembly.GetAssembly(typeof(Template)).GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(Template<>));

            Templates = templateClasses.Select(templateClass =>
            {
                try
                {
                    return (Template)Activator.CreateInstance(templateClass);
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not create instance of template {templateClass}. Ensure that it has a public parameterless constructor", e);
                }
            }).ToDictionary(t => t.TemplateName);
        }

        public static IReadOnlyCollection<string> TemplateNames => Templates.Keys.ToList();

        public static bool IsValidTemplateName(string name) => Templates.ContainsKey(name);

        public static Template GetTemplate(string templateName)
        {
            if (!IsValidTemplateName(templateName))
            {
                throw new ArgumentException($"Unrecognised template name: {templateName}");
            }

            return Templates[templateName];
        }
    }

    public abstract class Template<TOptions> : Template where TOptions : TemplateOptions
    {
        protected abstract IValidator<TOptions> OptionsValidator { get; }
        public abstract string GenerateDockerfile(ServiceConfiguration service, TOptions options, string configFileDirectory);

        public override Type OptionsType => typeof(TOptions);

        public override ValidationResult Validate(TemplateOptions options)
        {
            if (!(options is TOptions))
            {
                throw new InvalidOperationException($"Invalid options type for template {GetType()}. Expected {typeof(TOptions)}, got {options.GetType()}");
            }

            return OptionsValidator.Validate((TOptions) options);
        }

        public override string GenerateDockerfile(ServiceConfiguration service, string configFileDirectory)
        {
            if (!(service.TemplateOptions is TOptions))
            {
                throw new InvalidOperationException($"Invalid options type for template {GetType()}. Expected {typeof(TOptions)}, got {service.TemplateOptions.GetType()}");
            }

            return GenerateDockerfile(service, (TOptions) service.TemplateOptions, configFileDirectory);
        }
    }

    public abstract class TemplateOptions
    {
        public abstract string DockerBuildPath { get; }
    }
}
