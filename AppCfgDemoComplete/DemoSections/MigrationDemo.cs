using AppCfg.SettingStore;
using AppCfgDemoComplete.Helpers;
using System;
using System.IO;

namespace AppCfgDemoComplete.DemoSections
{
    /// <summary>
    /// Demonstrates moving secrets out of App.config into secrets.json with UserSecretsMigrator.
    /// This is the one-off step a .NET Framework project takes to start using
    /// MyAppCfg.Configure(userSecretsId: ...) without retyping every secret by hand.
    /// </summary>
    public static class MigrationDemo
    {
        private const string MigrationSecretsId = "appcfg-demo-migration";

        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "App.config → User Secrets Migration Demo",
                "Copies selected appSettings / connectionStrings from this app's App.config\n" +
                "into a secrets.json file, ready for MyAppCfg.Configure(userSecretsId: ...).\n" +
                "Existing secrets are kept unless OverwriteExisting = true."
            );

            try
            {
                OutputHelper.WriteHeader("Running migration");
                Console.WriteLine("  UserSecretsMigrator.MigrateFromCurrentConfig(");
                Console.WriteLine($"      \"{MigrationSecretsId}\",");
                Console.WriteLine("      new MigrationOptions { KeyFilter = k => k.StartsWith(\"ChainedDemo:\") || k == \"myConnFirst\" });");
                Console.WriteLine();

                var result = UserSecretsMigrator.MigrateFromCurrentConfig(
                    MigrationSecretsId,
                    new MigrationOptions
                    {
                        // In a real app you would typically filter on *Password*, *Key*, *Secret*, ...
                        KeyFilter = key => key.StartsWith("ChainedDemo:", StringComparison.OrdinalIgnoreCase)
                                        || key.Equals("myConnFirst", StringComparison.OrdinalIgnoreCase)
                    });

                OutputHelper.WriteSuccess($"Wrote {result.MigratedKeys.Count} key(s) to:");
                Console.WriteLine($"  {result.SecretsFilePath}");
                Console.WriteLine();

                Console.WriteLine("Migrated keys:");
                foreach (var key in result.MigratedKeys)
                {
                    Console.WriteLine($"  + {key}");
                }

                if (result.SkippedKeys.Count > 0)
                {
                    Console.WriteLine();
                    OutputHelper.WriteWarning($"{result.SkippedKeys.Count} key(s) already existed in secrets.json and were kept (run again with OverwriteExisting = true to replace):");
                    foreach (var key in result.SkippedKeys)
                    {
                        Console.WriteLine($"  = {key}");
                    }
                }

                Console.WriteLine();
                OutputHelper.WriteHeader("Resulting secrets.json");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(File.ReadAllText(result.SecretsFilePath));
                Console.ResetColor();

                OutputHelper.WriteSection("Other ways to migrate");
                Console.WriteLine("From a file instead of the running app (works for Web.config too):");
                Console.WriteLine("  UserSecretsMigrator.MigrateFromConfigFile(@\"C:\\src\\MyApp\\Web.config\", \"my-app-secrets\");");
                Console.WriteLine();
                Console.WriteLine("Without writing any code (Scripts/Migrate-AppConfigToUserSecrets.ps1):");
                Console.WriteLine("  .\\Migrate-AppConfigToUserSecrets.ps1 -ConfigPath .\\Web.config -UserSecretsId my-app-secrets -Include '*Password*','*Key'");
                Console.WriteLine();
                OutputHelper.WriteInfo("After migrating, delete the keys from App.config and call MyAppCfg.Configure(userSecretsId: \"...\").");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"(Demo output can be removed by deleting {Path.GetDirectoryName(result.SecretsFilePath)})");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run migration demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
