using System;

namespace AppCfgDemoComplete.Helpers
{
    public static class MenuHelper
    {
        public static void ShowMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════╗
║           AppCfg.Net - Complete Feature Demonstration         ║
║                   All-In-One Demo Project                     ║
╚═══════════════════════════════════════════════════════════════╝
");
            Console.ResetColor();

            Console.WriteLine("Please select a demo to run:\n");

            WriteMenuOption("1", "Basic Types Demo", "All primitive types + lists");
            WriteMenuOption("2", "JSON Configuration Demo", "Complex objects from JSON");
            WriteMenuOption("3", "Connection String Demo", "SqlConnectionStringBuilder");
            WriteMenuOption("4", "Custom Parser Demo", "ITypeParserRawBuilder (REGISTRATION)");
            WriteMenuOption("5", "Priority-Based Config Demo", "Automatic multi-source loading (RECOMMENDED)", true);
            WriteMenuOption("6", "Environment Variables Demo", "Environment variable store");
            WriteMenuOption("7", "User Secrets Demo", "User secrets store");
            WriteMenuOption("8", "Database Store Demo", "MSSQL custom store (REGISTRATION)");
            WriteMenuOption("9", "Redis Store Demo", "Redis custom store (REGISTRATION)");
            WriteMenuOption("10", "DefaultOption Demo", "DefaultOption attribute");
            WriteMenuOption("11", "Multi-Tenancy Demo", "Tenant-specific settings");
            WriteMenuOption("12", "Nested Settings Demo", "Nested interface configuration");
            WriteMenuOption("13", "Advanced Features Demo", "IReadOnlyList, RawValue");
            WriteMenuOption("14", "Error Handling Demo", "Missing values, type errors");
            WriteMenuOption("15", "Computed Settings Demo", "Derive values from config");

            Console.WriteLine();
            WriteMenuOption("A", "Run All Demos", "Execute all demos sequentially");
            WriteMenuOption("Q", "Quit", "Exit application");

            Console.Write("\n> Your choice: ");
        }

        private static void WriteMenuOption(string number, string title, string description, bool highlight = false)
        {
            if (highlight)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"  [{number}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{title} ★");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($" - {description}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"  [{number}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{title}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($" - {description}");
                Console.ResetColor();
            }
        }

        public static void ShowSectionHeader(string title, string description)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', 63) + "╗");
            Console.WriteLine($"║ {title.PadRight(61)} ║");
            Console.WriteLine("╚" + new string('═', 63) + "╝");
            Console.ResetColor();

            if (!string.IsNullOrEmpty(description))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"\n{description}\n");
                Console.ResetColor();
            }
        }

        public static void PauseForUser()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('─', 63));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Press any key to return to menu...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        public static void ShowFeatureDisabledMessage(string featureName, string configKey)
        {
            Console.WriteLine();
            OutputHelper.WriteWarning($"{featureName} is currently disabled.");
            Console.WriteLine();
            Console.WriteLine($"To enable this demo:");
            Console.WriteLine($"  1. Set '{configKey}' to 'true' in App.config");
            Console.WriteLine($"  2. Ensure the required service is running (SQL Server/Redis)");
            Console.WriteLine($"  3. Update connection string if needed");
            Console.WriteLine();
        }
    }
}
