using System;
using SharpYaml;

namespace Clud.Cli.Helpers
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ConfigParseException : Exception
    {
        public ConfigParseException(YamlException innerException)
            : base($"Your configuration file is invalid and could not be parsed: " + Environment.NewLine + innerException.Message, innerException)
        {
        }
    }
}
