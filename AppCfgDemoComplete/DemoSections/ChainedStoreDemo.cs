using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    /// <summary>
    /// ⭐ MOST IMPORTANT DEMO ⭐
    /// Demonstrates ChainedStore - the RECOMMENDED approach for production applications.
    /// Shows priority-based configuration loading from multiple sources.
    /// Priority order: Environment Variables → User Secrets → App.config
    /// </summary>
    public static class ChainedStoreDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "⭐ Priority-Based Configuration Demo ⭐",
                "This demonstrates the RECOMMENDED approach for production apps.\n" +
                "Simply call MyAppCfg.Configure() and ALL your settings automatically use:\n" +
                "  1. Environment Variables (highest priority)\n" +
                "  2. User Secrets\n" +
                "  3. App.config (fallback)\n\n" +
                "No [DefaultOption] attributes needed - it just works!"
            );

            try
            {
                // Initialize - one-liner setup makes priority-based loading the default!
                MySettings.InitializeChainedStore();

                OutputHelper.WriteInfo("Priority-based configuration enabled!");
                OutputHelper.WriteInfo("Environment variable prefix: APPCFG__");
                OutputHelper.WriteInfo("User secrets ID: appcfg-demo-complete");
                Console.WriteLine();

                // Get settings
                var settings = MyAppCfg.Get<IChainedSettings>();

                OutputHelper.WriteHeader("Configuration Values with Priority");
                Console.WriteLine("(Demo auto-initialized env vars + secrets.json on startup)\n");

                // AppConfigValue - should come from App.config (no override set)
                OutputHelper.WriteSetting("AppConfigValue", settings.AppConfigValue, "App.config (no override)");

                // SecretValue - overridden in secrets.json
                OutputHelper.WriteSetting("SecretValue", settings.SecretValue, "secrets.json (priority 2)");

                // EnvVarValue - overridden by environment variable
                OutputHelper.WriteSetting("EnvVarValue", settings.EnvVarValue, "ENV VAR (priority 1)");

                // OverrideMe - demonstrates the priority chain (env var wins over secrets)
                OutputHelper.WriteSetting("OverrideMe", settings.OverrideMe, "ENV VAR wins over secrets.json!");

                Console.WriteLine();
                OutputHelper.WriteSection("How to Test Priority Order");

                Console.WriteLine("1. App.config (Current state):");
                Console.WriteLine("   All values come from App.config by default");
                Console.WriteLine();

                Console.WriteLine("2. Add User Secret:");
                Console.WriteLine("   Create: %APPDATA%\\Microsoft\\UserSecrets\\appcfg-demo-complete\\secrets.json");
                Console.WriteLine("   Content:");
                Console.WriteLine("   {");
                Console.WriteLine("     \"ChainedDemo:SecretValue\": \"from-secrets-json\",");
                Console.WriteLine("     \"ChainedDemo:OverrideMe\": \"secrets-wins\"");
                Console.WriteLine("   }");
                Console.WriteLine("   Result: SecretValue and OverrideMe now come from secrets.json");
                Console.WriteLine();

                Console.WriteLine("3. Add Environment Variable (highest priority):");
                Console.WriteLine("   Set: APPCFG__ChainedDemo__EnvVarValue = from-environment");
                Console.WriteLine("   Set: APPCFG__ChainedDemo__OverrideMe = environment-wins");
                Console.WriteLine("   Result: EnvVarValue and OverrideMe now come from environment");
                Console.WriteLine();

                OutputHelper.WriteSuccess("Priority-based configuration is the recommended approach!");
                Console.WriteLine();
                Console.WriteLine("Benefits:");
                Console.WriteLine("  ✓ Keep secrets out of source control");
                Console.WriteLine("  ✓ Override settings per environment");
                Console.WriteLine("  ✓ Simple one-liner initialization");
                Console.WriteLine("  ✓ Clear priority order");
                Console.WriteLine("  ✓ No special attributes needed - it's automatic!");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run ChainedStore demo: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine($"Details: {ex}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
