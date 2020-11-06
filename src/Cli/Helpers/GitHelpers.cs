using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Clud.Cli.Helpers
{
    public class GitHelpers
    {
        public static async Task<string> CommitHash(string workingDirectory)
        {
            using var process = new Process
            {
                StartInfo =
                {
                    FileName = "git",
                    Arguments = "rev-parse HEAD",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
            };

            process.Start();
            process.WaitForExit(milliseconds: 5_000);

            if (process.ExitCode != 0)
            {
                return null;
            }
            else
            {
                return await process.StandardOutput.ReadLineAsync();
            }
        }
    }
}
