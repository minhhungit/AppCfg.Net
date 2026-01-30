using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    /// <summary>
    /// Demonstrates the Computed attribute for deriving property values from other settings.
    ///
    /// Computed properties are useful when you need to:
    /// - Combine multiple configuration values into one (e.g., connection strings)
    /// - Transform configuration values (e.g., URL building)
    /// - Calculate derived values (e.g., adjusted pool sizes)
    /// - Generate lists or complex objects from simple config values
    /// </summary>
    public static class ComputedSettingsDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Computed Settings Demo",
                "Derive property values from other configuration settings using static helper methods.\n" +
                "Computed properties are not loaded from config - they are calculated at runtime.");

            try
            {
                // Initialize chained store to enable env var overrides
                MySettings.InitializeChainedStore();

                ShowBasicComputedProperties();
                ShowComputedConnectionString();
                ShowComputedNumericValues();
                ShowComputedCollections();
                ShowCodeExample();
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Demo error: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }

        private static void ShowBasicComputedProperties()
        {
            OutputHelper.WriteHeader("1. Basic Computed Properties");
            Console.WriteLine("Computed properties combine multiple config values into derived values.\n");

            var settings = MyAppCfg.Get<IComputedSettings>();

            Console.WriteLine("Source configuration values:");
            OutputHelper.WriteSetting("Host", settings.Host, "ENV VAR override (demo)");
            OutputHelper.WriteSetting("Port", settings.Port, "App.config");
            OutputHelper.WriteSetting("Database", settings.Database, "App.config");

            Console.WriteLine("\nComputed values:");
            OutputHelper.WriteSetting("HostAddress", settings.HostAddress, "Computed");
            OutputHelper.WriteSetting("FullDatabasePath", settings.FullDatabasePath, "Computed");

            OutputHelper.WriteSuccess("Computed properties derived from config values!");
        }

        private static void ShowComputedConnectionString()
        {
            OutputHelper.WriteHeader("2. Connection String Builder");
            Console.WriteLine("Build complex connection strings from individual settings.\n");

            var settings = MyAppCfg.Get<IComputedSettings>();

            Console.WriteLine("Individual settings:");
            OutputHelper.WriteSetting("Host", settings.Host, "ENV VAR override");
            OutputHelper.WriteSetting("Port", settings.Port, "App.config");
            OutputHelper.WriteSetting("Database", settings.Database, "App.config");
            OutputHelper.WriteSetting("Username", settings.Username, "App.config");
            OutputHelper.WriteSetting("UseSSL", settings.UseSSL, "Default value");
            OutputHelper.WriteSetting("MaxPoolSize", settings.MaxPoolSize, "Default value");

            Console.WriteLine("\nComputed connection string:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  {settings.ConnectionString}");
            Console.ResetColor();

            OutputHelper.WriteSuccess("Connection string built from individual settings!");
        }

        private static void ShowComputedNumericValues()
        {
            OutputHelper.WriteHeader("3. Computed Numeric Values");
            Console.WriteLine("Computed properties can transform and calculate numeric values.\n");

            var settings = MyAppCfg.Get<IComputedSettings>();

            Console.WriteLine("Source value:");
            OutputHelper.WriteSetting("MaxPoolSize", settings.MaxPoolSize, "Default value");

            Console.WriteLine("\nComputed value:");
            OutputHelper.WriteSetting("EffectivePoolSize", settings.EffectivePoolSize, "Computed (MaxPoolSize * 2)");

            OutputHelper.WriteInfo("This pattern is useful for adjusting config values based on runtime conditions.");
        }

        private static void ShowComputedCollections()
        {
            OutputHelper.WriteHeader("4. Computed Collections");
            Console.WriteLine("Computed properties can return lists and complex objects.\n");

            var settings = MyAppCfg.Get<IComputedSettings>();

            Console.WriteLine("Common endpoints derived from Host, Port, and Database:");
            foreach (var endpoint in settings.CommonEndpoints)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  - {endpoint}");
                Console.ResetColor();
            }

            OutputHelper.WriteSuccess($"Generated {settings.CommonEndpoints.Count} endpoints from config values!");
        }

        private static void ShowCodeExample()
        {
            OutputHelper.WriteHeader("5. Code Example");
            Console.WriteLine("How to define computed properties:\n");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(@"// 1. Define your settings interface with regular and computed properties:
public interface IComputedSettings
{
    [Option(Alias = ""Computed:Host"")]
    string Host { get; }

    [Option(Alias = ""Computed:Port"")]
    int Port { get; }

    // Use [Computed] attribute with helper type and method name
    [Computed(typeof(ComputedHelpers), nameof(ComputedHelpers.GetHostAddress))]
    string HostAddress { get; }
}

// 2. Create a static helper class with computation methods:
public static class ComputedHelpers
{
    // Method must be: public, static, accept settings interface, return property type
    public static string GetHostAddress(IComputedSettings settings)
    {
        return $""{settings.Host}:{settings.Port}"";
    }
}

// 3. Use as normal - computed values are calculated automatically:
var settings = MyAppCfg.Get<IComputedSettings>();
Console.WriteLine(settings.HostAddress); // ""localhost:5432""");
            Console.ResetColor();

            Console.WriteLine();
            OutputHelper.WriteInfo("Computed properties are evaluated after all config properties are loaded.");
            OutputHelper.WriteInfo("They can access any property on the settings interface.");
        }
    }
}
