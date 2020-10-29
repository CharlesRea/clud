using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clud.Cli.Config;
using Clud.Cli.Helpers;
using Clud.Cli.Templates;
using Clud.Grpc;
using Google.Protobuf.Collections;
using Grpc.Net.Client;

namespace Clud.Cli.Commands
{
    public static class Deploy
    {
        public static Command Command()
        {
            var command = new Command("deploy", "Deploy an application to the clud server")
            {
                new Argument<string>("config", () => "clud.yaml")
                {
                    Description = $"The path to the clud config file'",
                    Arity = ArgumentArity.ZeroOrOne,
                },
                new Option<bool>("--update-secret-values")
                {
                    Description = "clud will prompt you to enter new values for existing secrets",
                    IsRequired = false,
                },
                new Option<bool>("--verbose")
                {
                    Description = "Displays additional output. Useful for debugging",
                    IsRequired = false,
                },
            };

            command.Handler = CommandHandler.Create<DeployCommandArguments>(Run);

            return command;
        }

        public class DeployCommandArguments
        {
            public IConsole Console { get; set; }
            public string Config { get; set; }
            public bool UpdateSecretValues { get; set; }
            public bool Verbose { get; set; }
        }

        private static async Task<int> Run(DeployCommandArguments args)
        {
            using var channel = GrpcChannel.ForAddress(CludOptions.ServerUri);
            var handler = new Handler(args, channel, new OutputContext(args.Verbose));
            return await handler.RunDeployment();
        }

        public class Handler
        {
            private readonly DeployCommandArguments args;
            private readonly GrpcChannel channel;
            private readonly OutputContext output;
            private readonly Applications.ApplicationsClient applicationsClient;
            private readonly Deployments.DeploymentsClient deploymentsClient;

            public Handler(DeployCommandArguments args, GrpcChannel channel, OutputContext output)
            {
                this.args = args;
                this.channel = channel;
                this.output = output;

                applicationsClient = new Applications.ApplicationsClient(channel);
                deploymentsClient = new Deployments.DeploymentsClient(channel);
            }

            public async Task<int> RunDeployment()
            {
                ConsoleHelpers.PrintLogo();

                var configuration = GetConfiguration();
                var services = await ProcessServices(configuration);

                output.Info("Contacting the clud server to kick off the deployment...");

                var response = await deploymentsClient.DeployApplicationAsync(new DeployCommand
                {
                    Name = configuration.Name.ToLowerInvariant(),
                    Owner = configuration.Owner,
                    Description = configuration.Description,
                    Repository = configuration.Repository,
                    EntryPoint = configuration.EntryPoint,
                    Services = { services },
                });

                LogSuccessfulDeployment(response);
                return 0;
            }

            private ApplicationConfiguration GetConfiguration()
            {
                if (!File.Exists(args.Config))
                {
                    throw new ValidationException($"Could not find a configuration file at path: {args.Config}. Specify the file location as an argument passed to clud ('clud deploy <file-location>')");
                }

                using var fileStream = File.OpenText(args.Config);
                var configuration = ApplicationConfiguration.Parse(fileStream);

                if (configuration.Errors.Any())
                {
                    throw new ValidationException(
                        $"Your configuration file is invalid:" +
                        Environment.NewLine +
                        string.Join(Environment.NewLine, configuration.Errors.Select(error => " - " + error))
                    );
                }

                if (configuration.Warnings.Any())
                {
                    output.Warning(string.Join(Environment.NewLine, configuration.Warnings));
                }

                output.Verbose("Configuration file was read successfully." + Environment.NewLine);

                return configuration.Result;
            }

            private async Task<List<DeployCommand.Types.Service>> ProcessServices(ApplicationConfiguration configuration)
            {
                var configFileDirectory = Directory.GetParent(args.Config).FullName;
                var existingAppSecrets = await applicationsClient.GetSecretsAsync(
                    new SecretsQuery { ApplicationName = configuration.Name }
                );

                var services = new List<DeployCommand.Types.Service>();
                foreach (var serviceConfig in configuration.Services)
                {
                    services.Add(await ProcessService(serviceConfig, configuration.Name, configFileDirectory, existingAppSecrets));
                }

                return services;
            }

            private async Task<DeployCommand.Types.Service> ProcessService(
                ServiceConfiguration service,
                string applicationName,
                string configFileDirectory,
                SecretsResponse existingAppSecrets
            )
            {
                output.Verbose($"Processing service '{service.Name}' ...");

                string dockerImage = null;
                var isPublicDockerImage = false;

                if (service.SpecifiesTemplate)
                {
                    dockerImage = await BuildDockerImageFromTemplate(service, applicationName, configFileDirectory);
                }
                else if (service.SpecifiesDockerfile)
                {
                    dockerImage = await BuildDockerImageFromDockerfile(service, applicationName, configFileDirectory);
                }
                else if (service.SpecifiesDockerImage)
                {
                    dockerImage = service.DockerImage;
                    isPublicDockerImage = true;
                }

                output.Verbose($"Successfully processed '{service.Name}'.");

                return new DeployCommand.Types.Service
                {
                    Name = service.Name,
                    DockerImage = dockerImage,
                    IsPublicDockerImage = isPublicDockerImage,
                    HttpPort = service.HttpPort,
                    TcpPorts = { service.TcpPorts },
                    UdpPorts = { service.UdpPorts },
                    Replicas = service.Replicas,
                    PersistentStoragePath = service.PersistentStoragePath,
                    EnvironmentVariables =
                    {
                        service.EnvironmentVariables.Select(pair => new DeployCommand.Types.EnvironmentVariable
                        {
                            Name = pair.Key,
                            Value = pair.Value,
                        })
                    },
                    Secrets =
                    {
                        await GetServiceSecrets(service, existingAppSecrets),
                    },
                };
            }

            private async Task<IReadOnlyCollection<DeployCommand.Types.Secret>> GetServiceSecrets(
                ServiceConfiguration service,
                SecretsResponse existingAppSecrets
            )
            {
                if (!service.Secrets.Any())
                {
                    return new List<DeployCommand.Types.Secret>();
                }

                var secrets = service.Secrets.Select(secret => new DeployCommand.Types.Secret
                {
                    Name = secret
                }).ToList();

                var existingServiceSecretNames = (existingAppSecrets.Services
                        .SingleOrDefault(s => s.Name == service.Name)
                        ?.Secrets ?? new RepeatedField<SecretsResponse.Types.Secret>())
                    .Select(s => s.Name)
                    .ToHashSet();

                foreach (var secret in secrets)
                {
                    var secretAlreadyExists = existingServiceSecretNames.Contains(secret.Name);
                    if (args.UpdateSecretValues || !secretAlreadyExists)
                    {
                        await Console.Out.WriteLineAsync(
                            $"Please enter a value for the {secret.Name} secret for service {service.Name}" +
                            (secretAlreadyExists ? " (leave blank to keep existing value unchanged)" : "") +
                            ":"
                        );
                        var value =  await Console.In.ReadLineAsync();

                        var isValid = secretAlreadyExists || !string.IsNullOrEmpty(value);
                        while (!isValid)
                        {
                            await Console.Out.WriteLineAsync(
                                $"Value for secret {secret.Name} must not be empty. Please enter a value again: "
                            );
                            value =  await Console.In.ReadLineAsync();
                            isValid = !string.IsNullOrEmpty(value);
                        }

                        secret.Value = string.IsNullOrEmpty(value) ? null : value;
                    }
                }

                return secrets;
            }

            public async Task<string> BuildDockerImageFromTemplate(
                ServiceConfiguration service,
                string applicationName,
                string configFileDirectory
            )
            {
                output.Info($"Processing service '{service.Name}' ...");

                var template = Template.GetTemplate(service.TemplateName);
                var dockerfile = template.GenerateDockerfile(service, configFileDirectory);

                var dockerFilePath = Path.Join(configFileDirectory, ".clud", "temp", service.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(dockerFilePath));
                await File.WriteAllTextAsync(dockerFilePath, dockerfile);

                return await ProcessDockerImage(service, applicationName, configFileDirectory, dockerFilePath);
            }

            private async Task<string> BuildDockerImageFromDockerfile(
                ServiceConfiguration service,
                string applicationName,
                string configFileDirectory
            )
            {
                output.Info($"Processing service '{service.Name}' ...");

                var dockerfilePath = Path.GetFullPath(service.Dockerfile, configFileDirectory);

                if (!File.Exists(dockerfilePath))
                {
                    throw new Exception($"Could not locate Dockerfile at '{dockerfilePath}' for service {service.Name}");
                }

                return await ProcessDockerImage(service, applicationName, configFileDirectory, dockerfilePath);
            }

            private async Task<string> ProcessDockerImage(
                ServiceConfiguration service,
                string applicationName,
                string configFileDirectory,
                string dockerFilePath
            )
            {
                output.Info("Building the Docker image ...");

                var dockerBuildPath = Path.GetFullPath(service.TemplateOptions.DockerBuildPath, configFileDirectory);

                var imageName = $"{applicationName.ToLowerInvariant()}-{service.Name.ToLowerInvariant()}";
                var tagVersion = Guid.NewGuid().ToString().Substring(0, 8);
                var imageTag = $"{imageName}:{tagVersion}";
                await CommandLineHelpers.ExecuteCommand($"docker build -t {imageName} -f {dockerFilePath} {dockerBuildPath}",
                    output);

                output.Success("Successfully built the Docker image.");

                output.Info("Pushing the Docker image to the clud registry ...");
                await CommandLineHelpers.ExecuteCommand($"docker tag {imageName} {CludOptions.Registry}/{imageTag}", output);
                await CommandLineHelpers.ExecuteCommand($"docker push {CludOptions.Registry}/{imageTag}", output);
                output.Success("Successfully pushed the Docker image.");

                output.Success($"Successfully processed '{service.Name}'.");
                output.Info();

                return imageTag;
            }

            private void LogSuccessfulDeployment(DeploymentResponse response)
            {
                output.Success($"Your deployment has begun! \\(^?^)/");
                output.Info();
                output.Success($"You can monitor the status of your deployment at {response.ManagementUrl}");
                output.Info();

                if (!string.IsNullOrWhiteSpace(response.IngressUrl))
                {
                    output.Success($"Your application will be available at {response.IngressUrl}");
                    output.Info();
                }

                foreach (var service in response.Services)
                {
                    var publicAccess = service.IngressUrl != null ? $"publicly at {service.IngressUrl}, and " : null;
                    var ports = service.Ports.Any()
                        ? ". " + string.Join(" ",
                            service.Ports.Select(port =>
                                port.ExposedPort != null
                                    ? $"Port {port.TargetPort} can be accessed publicly at {port.HostName}:{port.ExposedPort}."
                                    : $"Port {port.TargetPort} will be randomly assigned a port for public access. This will be shown on the management web UI once assigned."))
                        : "";
                    output.Success(
                        $"The service {service.Name} is accessible {publicAccess}from other clud services at the hostname {service.InternalHostname}{ports}");
                }

                output.Info();
            }
        }
    }
}
