using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;
using System.IO;

namespace AppCfgDemoComplete.DemoSections
{
    public static class UserSecretsDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "User Secrets Demo",
                "Demonstrates reading sensitive configuration from user secrets file.\n" +
                "User secrets are stored outside the project directory and excluded from source control.\n" +
                "Similar to .NET Core's Secret Manager tool."
            );

            try
            {
                MySettings.InitializeChainedStore();

                var secretsPath = GetSecretsFilePath();
                var secretsExist = File.Exists(secretsPath);

                OutputHelper.WriteHeader("User Secrets Configuration");
                Console.WriteLine($"Secrets file location:");
                Console.WriteLine($"  {secretsPath}");
                Console.WriteLine();

                if (secretsExist)
                {
                    OutputHelper.WriteSuccess("Secrets file found! (Auto-created by demo on startup)");
                    Console.WriteLine();

                    var settings = MyAppCfg.Get<ISecretSettings>();

                    OutputHelper.WriteHeader("Secret Values (from secrets.json)");
                    Console.Write("  ApiKey: ");
                    OutputHelper.MaskSecret(settings.ApiKey ?? "not-set");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" [secrets.json]");
                    Console.ResetColor();

                    Console.Write("  DatabasePassword: ");
                    OutputHelper.MaskSecret(settings.DatabasePassword ?? "not-set");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" [secrets.json]");
                    Console.ResetColor();

                    Console.Write("  EncryptionKey: ");
                    OutputHelper.MaskSecret(settings.EncryptionKey ?? "not-set");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" [secrets.json]");
                    Console.ResetColor();
                }
                else
                {
                    OutputHelper.WriteWarning("Secrets file not found!");
                    Console.WriteLine();
                    Console.WriteLine("To create user secrets:");
                    Console.WriteLine($"  1. Create directory: {Path.GetDirectoryName(secretsPath)}");
                    Console.WriteLine("  2. Create file: secrets.json");
                    Console.WriteLine("  3. Add JSON content:");
                    Console.WriteLine("     {");
                    Console.WriteLine("       \"Secret:ApiKey\": \"my-secret-api-key-12345\",");
                    Console.WriteLine("       \"Secret:DatabasePassword\": \"super-secure-password\",");
                    Console.WriteLine("       \"Secret:EncryptionKey\": \"encryption-key-xyz\"");
                    Console.WriteLine("     }");
                }

                Console.WriteLine();
                OutputHelper.WriteInfo("User secrets keep sensitive data out of source control!");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run UserSecrets demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }

        private static string GetSecretsFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Microsoft", "UserSecrets", "appcfg-demo-complete", "secrets.json");
        }
    }
}
