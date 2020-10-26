using System.Collections.Generic;
using FluentValidation;

namespace Clud.Cli.Config
{
    public static class ConfigValidators
    {
        public static IRuleBuilderOptions<T, string> ValidResourceName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(40).WithMessage("{PropertyName} must have fewer than 40 characters")
                .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]$").WithMessage("{PropertyName} must be a valid DNS subdomain name: it should contain alphanumeric characters and dashes (-) only");
        }
    }
}
