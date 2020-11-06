using System;
using Clud.Grpc;

namespace Clud.Api.Features
{
    public class Deployment
    {
        public int DeploymentId { get; private set; }
        public int ApplicationId { get; private set; }
        public int Version { get; private set; }
        public string CommitHash { get; private set; }
        public string ApplicationConfig { get; private set; }
        public DateTimeOffset DeploymentDateTime { get; private set; }

        private Deployment() { }

        public Deployment(Application application, DeployCommand command)
        {
            ApplicationId = application.ApplicationId;
            Version = command.Version;
            CommitHash = command.CommitHash;
            ApplicationConfig = command.ConfigurationYaml;
            DeploymentDateTime = DateTimeOffset.UtcNow;
        }
    }
}
