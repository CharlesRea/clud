using System;
using System.Reflection;
using CommandLine;
using DbUp;
using DbUp.Helpers;

namespace Migrations
{
    public class MigrationRunner
    {
        private const string CleanScriptsDirectory = "Clean";

        private class Arguments
        {
            [Option("connectionString", Required = true)]
            public string ConnectionString { get; set; }

            [Option("recreateDatabase", Default = false, HelpText = "Cleans the database and applies migrations from scratch. Valid values are true or false")]
            // Nullable bool to allow explicitly passing true or false as arguments - the Command Line Parser library uses switch semantics for bool params
            // instead of allowing passing as arguments - https://github.com/commandlineparser/commandline/issues/290
            public bool? RecreateDatabase { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Arguments>(args)
                .WithParsed(Run)
                .WithNotParsed(errors => Environment.Exit(-1));
        }

        private static void Run(Arguments args)
        {
            try
            {
                if (args.RecreateDatabase == true)
                {
                    CleanDatabase(args);
                }

                RunMigrations(args);
            }
            catch (Exception e)
            {
                ConsoleUtils.LogException(e);
                Environment.Exit(-1);
            }
        }

        private static void CleanDatabase(Arguments args)
        {
            Console.WriteLine("Cleaning existing database");
            var upgradeEngine = DeployChanges.To
                .PostgresqlDatabase(args.ConnectionString)
                .WithScriptsEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    script => script.Contains(CleanScriptsDirectory)
                )
                .WithExecutionTimeout(TimeSpan.FromMinutes(5))
                .WithTransactionPerScript()
                .LogToConsole()
                .JournalTo(new NullJournal())
                .Build();

            var result = upgradeEngine.PerformUpgrade();

            if (!result.Successful)
            {
                throw result.Error;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Cleaned database successfully");
            Console.ResetColor();
        }

        private static void RunMigrations(Arguments args)
        {
            var upgradeEngine = DeployChanges.To
                .PostgresqlDatabase(args.ConnectionString)
                .WithScriptsEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    script => !script.Contains(CleanScriptsDirectory)
                )
                .WithExecutionTimeout(TimeSpan.FromMinutes(2))
                .WithTransactionPerScript()
                .LogToConsole()
                .Build();

            var result = upgradeEngine.PerformUpgrade();

            if (!result.Successful)
            {
                throw result.Error;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Migrations applied successfully");
            Console.ResetColor();
        }
    }
}
