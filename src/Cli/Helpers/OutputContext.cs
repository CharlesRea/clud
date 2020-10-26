using System;
using System.Threading.Tasks;

namespace Clud.Cli.Helpers
{
    public class OutputContext
    {
        public bool IsVerbose { get; }

        public OutputContext(bool isVerbose)
        {
            IsVerbose = isVerbose;
        }

        public void Info()
        {
            Console.Out.WriteLine();
        }

        public void Info(string message)
        {
            Console.Out.WriteLine(message);
        }

        public void Verbose(string message)
        {
            if (IsVerbose)
            {
                Console.Out.WriteLine(message);
            }
        }

        public void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLine(message);
            Console.ResetColor();
        }

        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Out.WriteLine(message);
            Console.ResetColor();
        }

        public void Error(string message)
        {
            Console.Error.WriteLine(message);
        }

        public async Task<string> GetInputValue(string message)
        {
            await Console.Out.WriteLineAsync(message);
            return await Console.In.ReadLineAsync();
        }
    }
}
