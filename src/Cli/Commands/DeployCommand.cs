using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clud.Cli.Config;
using Clud.Cli.Helpers;
using Clud.Grpc;
using Grpc.Net.Client;
using SharpYaml.Serialization;

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
                new Option<bool>("--verbose")
                {
                    Description = "Displays additional output. Useful for debugging",
                    IsRequired = false,
                },
            };

            command.Handler = CommandHandler.Create<IConsole, string, bool>(RunDeployment);

            return command;
        }

        private static async Task<int> RunDeployment(IConsole console, string config, bool verbose)
        {
            var outputContext = new OutputContext(verbose);

            ConsoleHelpers.PrintLogo();

            var configuration = await GetConfiguration(config, outputContext);
            var configFileDirectory = Directory.GetParent(config).FullName;

            var serviceDeployments = new List<CreateDeploymentRequest.Types.ServiceDeploymentDetails>();

            foreach (var serviceConfiguration in configuration.Services)
            {
                var serviceDeploymentDetails = await ProcessService(serviceConfiguration, configFileDirectory, outputContext);
                serviceDeployments.Add(serviceDeploymentDetails);
            }

            using var channel = GrpcChannel.ForAddress(CludOptions.ServerUri);
            var client = new Deployments.DeploymentsClient(channel);

            var deploymentName = configuration.Name.ToLowerInvariant();

            outputContext.WriteInfo();
            outputContext.WriteInfo("Sending deployment details to the API ...");

            var createDeploymentRequest = new CreateDeploymentRequest
            {
                Name = deploymentName,
                Owner = configuration.Owner,
                Description = configuration.Description,
                Repository = configuration.Repository,
                EntryPoint = configuration.EntryPoint,
            };
            createDeploymentRequest.Services.AddRange(serviceDeployments);

            await client.CreateDeploymentAsync(createDeploymentRequest);

            outputContext.WriteSuccess("Successfully sent deployment details.");

            outputContext.WriteInfo();
            outputContext.WriteSuccess($"'{deploymentName}' has been deployed! \\(^?^)/");
            outputContext.WriteInfo();
            return 0;
        }

        private static async Task<Configuration> GetConfiguration(string config, OutputContext outputContext)
        {
            outputContext.WriteInfo("Attempting to read configuration file ...");

            if (!File.Exists(config))
            {
                throw new Exception("Could not locate configuration file");
            }

            await using var fileStream = File.OpenRead(config);
            var yamlSerializer = new Serializer(new SerializerSettings { NamingConvention = new CamelCaseNamingConvention() });
            var configuration = yamlSerializer.Deserialize<Configuration>(fileStream);

            var validationErrors = configuration?.GetValidationErrors().ToList();
            if (configuration == null || validationErrors.Any())
            {
                throw new Exception($"Invalid configuration file.\r\n{string.Join("\r\n", validationErrors?.Select(error => " - " + error) ?? new List<string>())}");
            }

            outputContext.WriteSuccess("Configuration file was read successfully.");
            outputContext.WriteInfo();

            return configuration;
        }

        private static async Task<CreateDeploymentRequest.Types.ServiceDeploymentDetails> ProcessService(ServiceConfiguration serviceConfiguration, string configFileDirectory, OutputContext outputContext)
        {
            outputContext.WriteInfo();
            outputContext.WriteInfo($"Processing service '{serviceConfiguration.Name}' ...");

            string dockerImage = null;
            var isPublicDockerImage = false;

            if (serviceConfiguration.SpecifiesProject)
            {
                outputContext.WriteInfo("The 'project' option was detected - clud will attempt to build a suitable image by inspecting your code.");
                throw new NotImplementedException("Producing a suitable Docker image from a project file is not supported yet");
            }
            else if (serviceConfiguration.SpecifiesDockerfile)
            {
                outputContext.WriteInfo("The 'dockerfile' option was detected - an image will be built and pushed to the remote registry.");
                dockerImage = await BuildDockerImageFromDockerfile(serviceConfiguration, configFileDirectory, outputContext);
            }
            else if (serviceConfiguration.SpecifiesDockerImage)
            {
                outputContext.WriteInfo("The 'dockerImage' option was detected - the name of the public image will be passed to the API.");
                dockerImage = serviceConfiguration.DockerImage;
                isPublicDockerImage = true;
            }

            outputContext.WriteInfo();
            outputContext.WriteSuccess($"Successfully processed '{serviceConfiguration.Name}'.");

            return new CreateDeploymentRequest.Types.ServiceDeploymentDetails
            {
                ServiceName = serviceConfiguration.Name,
                DockerImage = dockerImage,
                IsPublicDockerImage = isPublicDockerImage,
                Port = serviceConfiguration.Port,
            };
        }

        private static async Task<string> BuildDockerImageFromDockerfile(ServiceConfiguration configuration, string configFileDirectory, OutputContext outputContext)
        {
            outputContext.WriteInfo();
            outputContext.WriteInfo("Attempting to locate Dockerfile ...");

            var dockerfilePath = Path.GetFullPath(configuration.Dockerfile, configFileDirectory);

            if (!File.Exists(dockerfilePath))
            {
                throw new Exception($"Could not locate Dockerfile at '{dockerfilePath}'");
            }

            outputContext.WriteSuccess("Dockerfile was located successfully.");

            outputContext.WriteInfo();
            outputContext.WriteInfo("Building the Docker image ...");

            var dockerBuildPath = string.IsNullOrEmpty(configuration.DockerBuildPath)
                ? Directory.GetParent(dockerfilePath).FullName
                : Path.GetFullPath(configuration.DockerBuildPath, configFileDirectory);

            var imageName = configuration.Name.ToLowerInvariant();
            var tagVersion = Guid.NewGuid().ToString().Substring(0, 8);
            var imageTag = $"{imageName}:{tagVersion}";
            await CommandLineHelpers.ExecuteCommand($"docker build -t {imageName} -f {dockerfilePath} {dockerBuildPath}", outputContext);

            outputContext.WriteInfo();
            outputContext.WriteSuccess("Successfully built the Docker image.");

            outputContext.WriteInfo();
            outputContext.WriteInfo("Pushing the Docker image to the remote registry ...");
            await CommandLineHelpers.ExecuteCommand($"docker tag {imageName} {CludOptions.Registry}/{imageTag}", outputContext);
            await CommandLineHelpers.ExecuteCommand($"docker push {CludOptions.Registry}/{imageTag}", outputContext);
            outputContext.WriteInfo();
            outputContext.WriteSuccess("Successfully pushed the Docker image.");

            return imageTag;
        }
    }
}
