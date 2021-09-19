using DbUp;
using DbUp.Helpers;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace DbScripts
{
    class Program
    {
        static int Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];

            var scriptsUpgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly,s => s.StartsWith(typeof(Program).Namespace + ".Scripts", StringComparison.OrdinalIgnoreCase))
                    .JournalToSqlTable("dbo","SchemaVersions")
                    .LogToConsole()
                    .Build();

            var definitionsUpgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly, s => s.StartsWith(typeof(Program).Namespace + ".Definitions", StringComparison.OrdinalIgnoreCase))
                    .JournalTo(new NullJournal())
                    .LogToConsole()
                    .Build();

            var scriptsResults = scriptsUpgrader.PerformUpgrade();
            var definitionsResults = definitionsUpgrader.PerformUpgrade();
            if (!scriptsResults.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(scriptsResults.Error);
                Console.ResetColor();
                return -1;
            }

            if (!definitionsResults.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(definitionsResults.Error);
                Console.ResetColor();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}
