using AppCfgDemoComplete.DemoSections;
using AppCfgDemoComplete.Helpers;
using System;

namespace AppCfgDemoComplete
{
    /// <summary>
    /// AppCfg.Net Complete Demo - All-In-One Feature Demonstration
    ///
    /// This comprehensive demo showcases all features of AppCfg.Net:
    /// - 22+ built-in type parsers
    /// - Multiple configuration sources (App.config, Environment Variables, User Secrets)
    /// - ChainedStore for priority-based configuration (RECOMMENDED)
    /// - Custom parsers (ITypeParserRawBuilder)
    /// - Custom stores (Database, Redis)
    /// - Multi-tenancy support
    /// - Advanced features (IReadOnlyList, RawValue, DefaultOption)
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Initialize demo environment (env vars + secrets.json)
                DemoInitializer.Initialize();

                Console.WriteLine("Press any key to continue to the demo menu...");
                Console.ReadKey(true);

                RunInteractiveMenu();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nUnhandled exception: {ex.Message}");
                Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static void RunInteractiveMenu()
        {
            while (true)
            {
                MenuHelper.ShowMainMenu();
                var choice = Console.ReadLine()?.Trim().ToUpper();

                switch (choice)
                {
                    case "1":
                        BasicTypesDemo.Run();
                        break;

                    case "2":
                        JsonDemo.Run();
                        break;

                    case "3":
                        ConnectionStringDemo.Run();
                        break;

                    case "4":
                        CustomParserDemo.Run();
                        break;

                    case "5":
                        ChainedStoreDemo.Run();
                        break;

                    case "6":
                        EnvironmentVariablesDemo.Run();
                        break;

                    case "7":
                        UserSecretsDemo.Run();
                        break;

                    case "8":
                        DatabaseStoreDemo.Run();
                        break;

                    case "9":
                        RedisStoreDemo.Run();
                        break;

                    case "10":
                        DefaultOptionDemo.Run();
                        break;

                    case "11":
                        MultiTenancyDemo.Run();
                        break;

                    case "12":
                        NestedSettingsDemo.Run();
                        break;

                    case "13":
                        AdvancedFeaturesDemo.Run();
                        break;

                    case "14":
                        ErrorHandlingDemo.Run();
                        break;

                    case "15":
                        ComputedSettingsDemo.Run();
                        break;

                    case "16":
                        MigrationDemo.Run();
                        break;

                    case "A":
                        RunAllDemos();
                        break;

                    case "Q":
                        Console.WriteLine("\nThank you for exploring AppCfg.Net!");
                        return;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nInvalid choice. Please try again.");
                        Console.ResetColor();
                        System.Threading.Thread.Sleep(1000);
                        break;
                }
            }
        }

        static void RunAllDemos()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("                 RUNNING ALL DEMOS SEQUENTIALLY");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine("\nThis will run all 16 demos in order...");
            Console.WriteLine("Press any key to start, or ESC to cancel.");

            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Escape)
            {
                return;
            }

            var demos = new Action[]
            {
                BasicTypesDemo.Run,
                JsonDemo.Run,
                ConnectionStringDemo.Run,
                CustomParserDemo.Run,
                ChainedStoreDemo.Run,
                EnvironmentVariablesDemo.Run,
                UserSecretsDemo.Run,
                DatabaseStoreDemo.Run,
                RedisStoreDemo.Run,
                DefaultOptionDemo.Run,
                MultiTenancyDemo.Run,
                NestedSettingsDemo.Run,
                AdvancedFeaturesDemo.Run,
                ErrorHandlingDemo.Run,
                ComputedSettingsDemo.Run,
                MigrationDemo.Run
            };

            var demoNames = new string[]
            {
                "Basic Types",
                "JSON Configuration",
                "Connection Strings",
                "Custom Parser",
                "Priority-Based Configuration ⭐",
                "Environment Variables",
                "User Secrets",
                "Database Store",
                "Redis Store",
                "DefaultOption",
                "Multi-Tenancy",
                "Nested Settings",
                "Advanced Features",
                "Error Handling",
                "Computed Settings",
                "App.config → User Secrets Migration"
            };

            for (int i = 0; i < demos.Length; i++)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[{i + 1}/{demos.Length}] Running: {demoNames[i]}");
                Console.ResetColor();
                Console.WriteLine();

                try
                {
                    demos[i]();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nDemo failed: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to continue to next demo...");
                    Console.ReadKey(true);
                }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ All demos completed!");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
        }
    }
}
