using System;
using System.Diagnostics;
using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Helm;
using Nuke.Common.Tools.Kubernetes;

namespace Clud.Build
{
    public enum Environment
    {
        LocalDev,
        Production,
    }

    public class Build : NukeBuild
    {
        public static int Main()
        {
            return Execute<Build>(x => x.Deploy);
        }

        [Parameter] readonly string SqlConnectionString;
        [Parameter] readonly Environment Environment = Environment.LocalDev;
        [Parameter] readonly AbsolutePath KubeConfigPath;

        readonly string Configuration = "Release";

        AbsolutePath SourceDirectory => RootDirectory / "src";
        AbsolutePath MigrationsDirectory => SourceDirectory / "Migrations";

        AbsolutePath RootInfrastructureDirectory => RootDirectory / "Infrastructure";

        string EnvironmentInfrastructureDirectoryName => Environment switch
        {
            Environment.LocalDev =>  "dev",
            Environment.Production => "prod",
            _ => throw new InvalidOperationException("Unrecognised environment")
        };

        AbsolutePath EnvironmentInfrastructureDirectory =>
            RootInfrastructureDirectory / EnvironmentInfrastructureDirectoryName;

        AbsolutePath MigrationsDllFile =>
            MigrationsDirectory / $"bin/{Configuration}/netcoreapp3.1/Migrations.dll";

        public string SqlConnectionStringOrDefault =>
            SqlConnectionString ?? (Environment == Environment.LocalDev
                ? "Host=clud.local;Port=30432;Database=clud;Username=clud;Password=supersecret"
                : null);

        string DockerRegistry => Environment switch
        {
            Environment.LocalDev => "localhost:5002",
            Environment.Production => "registry.clud.ghyston.com",
            _ => throw new InvalidOperationException("Unrecognised environment")
        };

        Target CompileMigrations => _ => _
            .Executes(() =>
            {
                DotNetTasks.DotNetBuild(s => s
                    .SetProjectFile(MigrationsDirectory / "Migrations.csproj")
                    .SetConfiguration(Configuration)
                    .EnableTreatWarningsAsErrors()
                );
            });

        Target MigrateDatabase => _ => _
            .DependsOn(CompileMigrations)
            .Requires(() => SqlConnectionStringOrDefault != null)
            .Executes(() =>
            {
                DotNetTasks.DotNet(MigrationsDllFile + $" --connectionString \"{SqlConnectionStringOrDefault}\"");
            });

        Target RebuildDatabase => _ => _
            .DependsOn(CompileMigrations)
            .Requires(() => SqlConnectionStringOrDefault != null)
            .Executes(() =>
            {
                DotNetTasks.DotNet(MigrationsDllFile +
                                   $" --connectionString \"{SqlConnectionStringOrDefault}\" " +
                                   $"--recreateDatabase true"
                );
            });

        Target CreateCludNamespace => _ => _
            .Executes(() =>
            {
                KubernetesTasks.Kubernetes("create namespace clud");
            });

        Target ConfigureHelm => _ => _
            .Executes(() =>
            {
                HelmTasks.Helm("repo add stable https://kubernetes-charts.storage.googleapis.com/");
                HelmTasks.HelmRepoUpdate();
            });

        Target CreateRegistry => _ => _
            .DependsOn(ConfigureHelm)
            .After(CreateCludNamespace)
            .Executes(() =>
            {
                HelmTasks.Helm($"install docker-registry stable/docker-registry -f {EnvironmentInfrastructureDirectory / "registry-helm-values.yaml"} -n clud {KubeConfigArgument}");
            });

        readonly string imageTag = Guid.NewGuid().ToString().Substring(0, 8);

        Target PushDockerImage => _ => _
            .After(CreateRegistry)
            .Executes(() =>
            {
                DockerTasks.Docker($"build {RootDirectory} -t {DockerRegistry}/clud-server:{imageTag}");
                DockerTasks.Docker($"push {DockerRegistry}/clud-server:{imageTag}");
            });

        Target DeployCludInfrastructure => _ => _
            .After(CreateRegistry, PushDockerImage, CreateCludNamespace)
            .Before(MigrateDatabase, RebuildDatabase)
            .Executes(() =>
            {
                TextTasks.WriteAllText(RootInfrastructureDirectory / "build" / "kustomization.yaml", $@"
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
bases:
- ../{EnvironmentInfrastructureDirectoryName}
images:
- name: localhost:5000/clud-server
  newTag: {imageTag}
");
                KubernetesTasks.Kubernetes($"apply -k {RootInfrastructureDirectory / "build"} {KubeConfigArgument}");
            });

        Target Deploy => _ => _
            .DependsOn(PushDockerImage, DeployCludInfrastructure, MigrateDatabase);

        Target CreateLocalInfrastructure => _ => _
            .DependsOn(CreateCludNamespace, CreateRegistry, DeployCludInfrastructure);

        AbsolutePath ValidateKubeConfigPath()
        {
            var configPath = KubeConfigPath ?? (Environment == Environment.Production
                ? RootDirectory / "prod-kube-config"
                : null);

            if (configPath == null)
            {
                return null;
            }

            ControlFlow.Assert(
                FileSystemTasks.FileExists(configPath),
                $"kubeconfig file not found at {configPath}. Either ensure this file exists, or manually specify a KubeConfigPath argument");

            return configPath;
        }

        string KubeConfigArgument => ValidateKubeConfigPath() != null ? $"--kubeconfig={ValidateKubeConfigPath()}" : "";

        /* Unfortunately, on Windows, our local Docker engine cannot directly talk to Minikube (as they're in different HyperV
         * containers). So we need to expose the registry to our local network with `kubectl port-forward`, and then create a
         * Docker image to proxy the network call within the Docker engine back to our host network. (Based on
         * https://minikube.sigs.k8s.io/docs/handbook/registry/))
         */
        Target RegistryProxy => _ => _
            .Executes(() =>
            {
                IProcess portForward = null;
                IProcess dockerProxy = null;

                try
                {
                    portForward = ProcessTasks.StartProcess("kubectl",
                        "port-forward -n clud service/docker-registry 5002:5000");
                    dockerProxy = ProcessTasks.StartProcess(
                        "docker",
                        "run --rm -it --network=host alpine ash -c \"apk add socat && socat TCP-LISTEN:5002,reuseaddr,fork TCP:host.docker.internal:5002\"");

                    portForward.AssertZeroExitCode();
                    dockerProxy.AssertZeroExitCode();
                }
                finally
                {
                    portForward?.Kill();
                    dockerProxy?.Kill();
                    portForward?.Dispose();
                    dockerProxy?.Dispose();
                }
            });
    }
}
