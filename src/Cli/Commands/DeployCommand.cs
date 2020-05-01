using System;
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
        private const string CommandName = "deploy";

        private const string ConfigArgumentName = "config";
        private const string DefaultConfigFile = "clud.yml";

        public static Command Command()
        {
            var command = new Command(CommandName, "Deploy an application to the clud server");

            var configArgument = new Argument(ConfigArgumentName)
            {
                Description = $"The path to the clud config file. Defaults to '{DefaultConfigFile}'",
                Arity = ArgumentArity.ZeroOrOne,
            };
            configArgument.SetDefaultValue(DefaultConfigFile);
            command.AddArgument(configArgument);

            var verboseOption = new Option<bool>("--verbose")
            {
                Description = "Displays additional output. Useful for debugging",
                Required = false,
            };
            command.AddOption(verboseOption);

            command.Handler = CommandHandler.Create<IConsole, string, bool>(async (console, config, verbose) =>
            {
                ConsoleHelpers.PrintLogo();

                var configuration = await GetConfiguration(config);

                string dockerImage = null;
                var isPublicDockerImage = false;

                if (configuration.SpecifiesProject)
                {
                    Console.Out.WriteLine("The 'project' option was detected - clud will attempt to build a suitable image by inspecting your code.");
                    throw new NotImplementedException("Producing a suitable Docker image from a project file is not supported yet");
                }
                if (configuration.SpecifiesDockerfile)
                {
                    Console.Out.WriteLine("The 'dockerfile' option was detected - an image will be built and pushed to the remote registry.");
                    dockerImage = await BuildDockerImageFromDockerfile(config, configuration, verbose);
                }
                if (configuration.SpecifiesDockerImage)
                {
                    Console.Out.WriteLine("The 'dockerImage' option was detected - the name of the public image will be passed to the API.");
                    dockerImage = configuration.DockerImage;
                    isPublicDockerImage = true;
                }

                using var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new Deployments.DeploymentsClient(channel);

                var deploymentName = configuration.Name.ToLowerInvariant();

                Console.Out.WriteLine();
                Console.Out.WriteLine("Sending deployment details to the API ...");

                await client.CreateDeploymentAsync(new CreateDeploymentRequest
                {
                    Name = deploymentName,
                    DockerImage = dockerImage,
                    IsPublicDockerImage = isPublicDockerImage,
                    Port = configuration.Port,
                    Description = configuration.Description,
                    Owner = configuration.Owner,
                    Repository = configuration.Repository,
                });

                ConsoleHelpers.WriteSuccess("Successfully sent deployment details.");

                Console.Out.WriteLine();
                ConsoleHelpers.WriteSuccess($"'{deploymentName}' has been deployed! \\(^?^)/");
                Console.Out.WriteLine();
                return 0;
            });

            return command;
        }

        private static async Task<Configuration> GetConfiguration(string config)
        {
            Console.Out.WriteLine("Attempting to read configuration file ...");

            if (!File.Exists(config))
            {
                throw new Exception("Could not locate configuration file");
            }

            await using var fileStream = File.OpenRead(config);
            var yamlSerializer = new Serializer(new SerializerSettings { NamingConvention = new CamelCaseNamingConvention() });
            var configuration = yamlSerializer.Deserialize<Configuration>(fileStream);

            var validationErrors = configuration.GetValidationErrors();
            if (validationErrors.Any())
            {
                throw new Exception($"Invalid configuration file.\r\n{string.Join("\r\n", validationErrors.Select(error => " - " + error))}");
            }

            ConsoleHelpers.WriteSuccess("Configuration file was read successfully.");
            Console.Out.WriteLine();

            return configuration;
        }

        private static async Task<string> BuildDockerImageFromDockerfile(string config, Configuration configuration, bool verbose)
        {
            Console.Out.WriteLine();
            Console.Out.WriteLine("Attempting to locate Dockerfile ...");

            var configFileDirectory = Directory.GetParent(config).FullName;
            var dockerfilePath = Path.GetFullPath(configuration.Dockerfile, configFileDirectory);

            if (!File.Exists(dockerfilePath))
            {
                throw new Exception($"Could not locate Dockerfile at '{dockerfilePath}'");
            }

            ConsoleHelpers.WriteSuccess("Dockerfile was located successfully.");

            Console.Out.WriteLine();
            Console.Out.WriteLine("Building the Docker image ...");

            var dockerBuildPath = string.IsNullOrEmpty(configuration.DockerBuildPath)
                ? Directory.GetParent(dockerfilePath).FullName
                : Path.GetFullPath(configuration.DockerBuildPath, configFileDirectory);

            var imageName = configuration.Name.ToLowerInvariant();
            var tag = Guid.NewGuid().ToString().Substring(0, 8);
            await CommandLineHelpers.ExecuteCommand($"docker build -t {imageName} -f {dockerfilePath} {dockerBuildPath}", verbose);

            Console.Out.WriteLine();
            ConsoleHelpers.WriteSuccess("Successfully built the Docker image.");

            Console.Out.WriteLine();
            Console.Out.WriteLine("Pushing the Docker image to the remote registry ...");
            const string registryLocation = "registry.clud:5002"; // TODO - Parameterize per environment
            await CommandLineHelpers.ExecuteCommand($"docker tag {imageName} {registryLocation}/{imageName}:{tag}", verbose);
            await CommandLineHelpers.ExecuteCommand($"docker push {registryLocation}/{imageName}:{tag}", verbose);
            Console.Out.WriteLine();
            ConsoleHelpers.WriteSuccess("Successfully pushed the Docker image.");

           return $"{imageName}:{tag}";
        }
    }
}