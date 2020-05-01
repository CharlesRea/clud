using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Clud.Cli.Helpers
{
    public static class CommandLineHelpers
    {
        public static async Task ExecuteCommand(string command, bool verbose)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.Out.WriteLine();
            Console.Out.WriteLine(command);

            Console.ForegroundColor = ConsoleColor.DarkGray;

            var exitCode = await Process.ExecuteAsync(
                command: "cmd.exe",
                args: $"/C {command}",
                stdOut: verbose ? (Action<string>) Console.Out.WriteLine : null,
                stdErr: Console.Error.WriteLine
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
