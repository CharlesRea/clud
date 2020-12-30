namespace Clud.Api.Features
{
    public static class KubeNaming
    {
        public const string AppLabelKey = "App";
        public const string HttpPortName = "http";
        public const string EntryPointIngressName = "entry-point";
        public const string DockerRegistryLocation = "localhost:5000";

        public static string ImageNameWithoutCludRegistryUrl(string imageName) =>
            imageName != null && imageName.StartsWith(DockerRegistryLocation + "/")
                ? imageName.Substring(DockerRegistryLocation.Length + 1)
                : imageName;
    }
}
