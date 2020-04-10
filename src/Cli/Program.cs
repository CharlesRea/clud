using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace Clud.Cli
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var command = new RootCommand();

            command.Add(DeployCommand());

            command.Description = "Clud deployment tool";

            // Show commandline help unless a subcommand was used.
            command.Handler = CommandHandler.Create<IHelpBuilder>(help =>
            {
                help.Write(command);
                return 1;
            });

            command.Name = "clud";


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

        private static Command DeployCommand()
        {
            var command = new Command("deploy", "Deploy an application to the Clud server")
            {
            };

            command.AddArgument(new Argument("config")
            {
                Description = "The path to the Clud config file. Defaults to clud.yml",
                Arity = ArgumentArity.ZeroOrOne
            });

            command.Handler = CommandHandler.Create<IConsole, string>(async (console, configPath) =>
            {
                var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new Pods.PodsClient(channel);
                var pods = await client.ListPodsAsync(new PodsRequest());

                console.Out.Write("Pods:" + Environment.NewLine + string.Join<string>(Environment.NewLine, pods.Pods.Select(pod => "\t" + pod.Name)));

                return 0;
            });

            return command;
        }


        private static void HandleException(Exception exception, InvocationContext context)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            context.Console.Error.WriteLine("Uh oh, something failed. Badly.");
            context.Console.Error.WriteLine(exception.ToString());

            Console.ResetColor();

            context.ResultCode = 1;
        }
    }
}
