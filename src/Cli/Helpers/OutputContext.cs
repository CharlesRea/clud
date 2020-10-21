using System;

namespace Clud.Cli.Helpers
{
    public class OutputContext
    {
        public bool Verbose { get; }

        public OutputContext(bool verbose)
        {
            Verbose = verbose;
        }

        public void WriteInfo()
        {
            Console.Out.WriteLine();
        }

        public void WriteInfo(string message)
        {
            Console.Out.WriteLine(message);
        }

        public void WriteVerbose(string message)
        {
            if (Verbose)
            {
                Console.Out.WriteLine(message);
            }
        }

        public void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLine(message);
            Console.ResetColor();
        }

        public void WriteError(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
