using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Clud.Cli.Helpers;

namespace Clud.Cli
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var command = new RootCommand
            {
                Commands.Deploy.Command(),
                // ...add more commands here
            };

            command.Name = "clud";
            command.Description = "clud deployment tool";

            // Show commandline help unless a sub-command was used.
            command.Handler = CommandHandler.Create<IHelpBuilder>(help =>
            {
                ConsoleHelpers.PrintLogo();
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

        private static void HandleException(Exception exception, InvocationContext context)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            context.Console.Error.WriteLine("Uh oh, something has gone badly wrong :(");
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
