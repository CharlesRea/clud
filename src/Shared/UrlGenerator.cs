using Microsoft.Extensions.Options;

namespace Shared
{
    public class UrlGenerator
    {
        private readonly CludOptions cludOptions;

        public UrlGenerator(IOptions<CludOptions> cludOptions)
        {
            this.cludOptions = cludOptions.Value;
        }

        private const string rootApplicationName = "clud";

        private static bool IsRootApplicationName(string applicationName) => rootApplicationName == applicationName;

        public string GetApplicationUrl(string applicationName)
        {
            return IsRootApplicationName(applicationName)
                ? $"http://{cludOptions.BaseHostname}"
                : $"http://{applicationName}.{cludOptions.BaseHostname}";
        }

        public string GetApplicationUrlSuffix(string applicationName)
        {
            return IsRootApplicationName(applicationName)
                ? cludOptions.BaseHostname.Remove(0, rootApplicationName.Length)
                : $".{cludOptions.BaseHostname}";
        }

        public string GetExternalServiceHostname(string applicationName, string serviceName)
        {
            return $"{serviceName}.{applicationName}.{cludOptions.BaseHostname}";
        }

        public string GetInternalServiceHostname(string applicationName, string serviceName, int port)
        {
            return $"{serviceName}.{applicationName}:{port}";
        }

        public string GetServiceUrlSuffix(string applicationName) => $".{GetApplicationUrl(applicationName)}";
    }
}
