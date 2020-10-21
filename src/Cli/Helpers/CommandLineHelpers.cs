using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Clud.Cli.Helpers
{
    public static class CommandLineHelpers
    {
        public static async Task ExecuteCommand(string command, OutputContext outputContext)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            outputContext.WriteInfo();
            outputContext.WriteInfo(command);

            Console.ForegroundColor = ConsoleColor.DarkGray;

            var exitCode = await Process.ExecuteAsync(
                command: "cmd.exe",
                args: $"/C {command}",
                stdOut: outputContext.WriteVerbose,
                stdErr: outputContext.WriteError
            );

            Console.ResetColor();

            if (exitCode != 0)
            {
                throw new CommandFailedException(command);
            }
        }
    }

    public class CommandFailedException : Exception
    {
        public CommandFailedException(string command)
            : base($"An error occurred while executing the following command: '{command}'") { }
    }
}
