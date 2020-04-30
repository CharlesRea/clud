using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Clud.Grpc;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using Process = System.CommandLine.Invocation.Process;

namespace Clud.Cli
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var command = new RootCommand();

            command.Add(DeployCommand());

            command.Name = "clud";
            command.Description = "Clud deployment tool";

            // Show commandline help unless a subcommand was used.
            command.Handler = CommandHandler.Create<IHelpBuilder>(help =>
            {
                help.Write(command);
                return 1;
            });

            var builder = new CommandLineBuilder(command);
            builder.UseHelp();
            builder.UseVersionOption();
            builder.UseDebugDirective();
            builder.UseParseErrorReporting();
            builder.ParseResponseFileAs(ResponseFileHandling.ParseArgsAsSpaceSeparated);

            builder.CancelOnProcessTermination();
            builder.UseExceptionHandler(HandleException);

            var parser = builder.Build();
            return await parser.InvokeAsync(args);
        }

        private class ConfigFileSchema
        {
            public string Name { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Owner { get; set; } = string.Empty;
            public string Repository { get; set; } = string.Empty;

            public int Port { get; set; }

            public string DockerImage { get; set; }

            public string Dockerfile { get; set; }
            public string DockerBuildPath { get; set; } // Defaults to the directory containing the Dockerfile

            public string Project { get; set; }

            public List<string> GetValidationErrors()
            {
                var errors = new List<string>();

                if (string.IsNullOrEmpty(Name))
                {
                    errors.Add("You must specify a name");
                }

                if (new[] { DockerImage, Dockerfile, Project }.Count(value => !string.IsNullOrEmpty(value)) != 1)
                {
                    errors.Add("You must specify either a dockerImage, a dockerfile, or a project");
                }

                if (!string.IsNullOrEmpty(DockerBuildPath) && string.IsNullOrEmpty(Dockerfile))
                {
                    errors.Add("You should only specify a dockerBuildPath when also specifying a dockerfile");
                }

                return errors;
            }
        }

        // The YAML parsing library we're using doesn't offer this convention by default for some reason, so we have to roll our own
        private class CamelCaseNamingConvention : IMemberNamingConvention
        {
            public StringComparer Comparer => StringComparer.Ordinal;

            public string Convert(string name)
            {
                return char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
        }

        private static Command DeployCommand()
        {
            var command = new Command("deploy", "Deploy an application to the Clud server")
            {
            };

            const string defaultConfigFile = "clud.yaml";
            var configArgument = new Argument("config")
            {
                Description = $"The path to the Clud config file. Defaults to {defaultConfigFile}",
                Arity = ArgumentArity.ZeroOrOne,
            };
            configArgument.SetDefaultValue(defaultConfigFile);
            command.AddArgument(configArgument);

            command.Handler = CommandHandler.Create<IConsole, string>(async (console, config) =>
            {
                Console.Out.Write(@"       _             _     
  ___ | | _   _   __| |       __   _
 / __|| || | | | / _` |     _(  )_( )_
| (__ | || |_| || (_| |    (_   _    _)
 \___||_| \__,_| \__,_|      (_) (__)
");
                Console.Out.WriteLine();

                Console.Out.WriteLine();

                Console.Out.WriteLine("Attempting to read configuration file ...");

                if (!File.Exists(config))
                {
                    throw new Exception("Could not locate configuration file");
                }

                await using var fileStream = File.OpenRead(config);
                var yamlSerializer = new Serializer(new SerializerSettings { NamingConvention = new CamelCaseNamingConvention() });
                var parsedConfig = yamlSerializer.Deserialize<ConfigFileSchema>(fileStream);

                var validationErrors = parsedConfig.GetValidationErrors();
                if (validationErrors.Any())
                {
                    throw new Exception($"Invalid configuration file.\r\n{string.Join("\r\n", validationErrors.Select(error => " - " + error))}");
                }

                WriteSuccess("Configuration file was read successfully.");
                Console.Out.WriteLine();

                string dockerImage = null;
                var isPublicDockerImage = false;

                if (!string.IsNullOrEmpty(parsedConfig.Project))
                {
                    throw new NotImplementedException("Producing a suitable Docker image from a project file is not supported yet");
                }
                if (!string.IsNullOrEmpty(parsedConfig.Dockerfile))
                {
                    Console.Out.WriteLine("The 'dockerfile' option was detected - an image will be built and pushed to the remote registry.");

                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Attempting to locate Dockerfile ...");

                    var configFileDirectory = Directory.GetParent(config).FullName;
                    var dockerfilePath = Path.GetFullPath(parsedConfig.Dockerfile, configFileDirectory);

                    if (!File.Exists(dockerfilePath))
                    {
                        throw new Exception($"Could not locate Dockerfile at '{dockerfilePath}'");
                    }

                    WriteSuccess("Dockerfile was located successfully.");

                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Building the Docker image ...");

                    var dockerBuildPath = string.IsNullOrEmpty(parsedConfig.DockerBuildPath)
                        ? Directory.GetParent(dockerfilePath).FullName
                        : Path.GetFullPath(parsedConfig.DockerBuildPath, configFileDirectory);

                    var imageName = parsedConfig.Name.ToLowerInvariant();
                    var tag = Guid.NewGuid().ToString().Substring(0, 8);
                    await ExecuteCommand($"docker build -t {imageName} -f {dockerfilePath} {dockerBuildPath}");

                    Console.Out.WriteLine();
                    WriteSuccess("Successfully built the Docker image.");

                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Pushing the Docker image to the remote registry ...");
                    const string registryLocation = "registry.clud:5002"; // TODO - Parameterize per environment
                    await ExecuteCommand($"docker tag {imageName} {registryLocation}/{imageName}:{tag}");
                    await ExecuteCommand($"docker push {registryLocation}/{imageName}:{tag}");
                    Console.Out.WriteLine();
                    WriteSuccess("Successfully pushed the Docker image.");

                    dockerImage = $"{imageName}:{tag}";
                }
                if (!string.IsNullOrEmpty(parsedConfig.DockerImage))
                {
                    Console.Out.WriteLine("The 'dockerImage' option was detected - the name of the public image will be passed to the API.");
                    dockerImage = parsedConfig.DockerImage;
                    isPublicDockerImage = true;
                }

                using var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new Deployments.DeploymentsClient(channel);

                var deploymentName = parsedConfig.Name.ToLowerInvariant();

                Console.Out.WriteLine();
                Console.Out.WriteLine("Sending deployment details to the API ...");

                await client.CreateDeploymentAsync(new CreateDeploymentRequest
                {
                    Name = deploymentName,
                    DockerImage = dockerImage,
                    IsPublicDockerImage = isPublicDockerImage,
                    Port = parsedConfig.Port,
                    Description = parsedConfig.Description,
                    Owner = parsedConfig.Owner,
                    Repository = parsedConfig.Repository,
                });

                WriteSuccess("Successfully sent deployment details.");

                Console.Out.WriteLine();
                WriteSuccess($"'{deploymentName}' has been deployed! \\(^?^)/");
                Console.Out.WriteLine();
                return 0;
            });

            return command;
        }

        private static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLine(message);
            Console.ResetColor();
        }

        private static async Task ExecuteCommand(string command)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.Out.WriteLine();
            Console.Out.WriteLine(command);

            Console.ForegroundColor = ConsoleColor.DarkGray;

            var exitCode = await Process.ExecuteAsync(
                command: "cmd.exe",
                args: $"/C {command}",
                stdOut: Console.Out.WriteLine,
                stdErr: Console.Error.WriteLine
            );

            Console.ResetColor();

            if (exitCode != 0)
            {
                throw new Exception($"An error occurred while executing the following command: '{command}'");
            }
        }

        private static void HandleException(Exception exception, InvocationContext context)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            context.Console.Error.WriteLine("Uh oh, something has gone badly wrong (ノ・∀・)ノ");
            context.Console.Error.WriteLine(exception.Message);

#if DEBUG
            context.Console.Error.WriteLine(exception.ToString());

            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                context.Console.Error.WriteLine("Inner exception:");
                context.Console.Error.WriteLine(exception.ToString());
            }
#endif

            Console.ResetColor();

            context.ResultCode = 1;
        }
    }
}
