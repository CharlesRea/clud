using System;
using Clud.Cli.Config;
using Microsoft.Extensions.Configuration;

namespace Clud.Cli
{
    public class CludOptions
    {
        private static readonly IConfigurationRoot configuration = BuildConfiguration();

        private static IConfigurationRoot BuildConfiguration()
        {
            var environmentName = Environment.GetEnvironmentVariable("CLUD_ENVIRONMENT");

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true);

            return configBuilder.Build();
        }

        public static string ServerUri => configuration["serverUri"];
        public static string Registry => configuration["registry"];
    }
}
