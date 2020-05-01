using System;
using SharpYaml.Serialization;

namespace Clud.Cli.Config
{
    // The YAML parsing library we're using doesn't offer this convention by default for some reason, so we have to roll our own
    public class CamelCaseNamingConvention : IMemberNamingConvention
    {
        public StringComparer Comparer => StringComparer.Ordinal;

        public string Convert(string name)
        {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
