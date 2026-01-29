using System;

namespace AppCfgDemoUserSecrets
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             Please create `secrets.json` file with bellow content in "C:\Users\minhhungit\AppData\Roaming\Microsoft\UserSecrets\appcfg-demo-secrets\secrets.json"

            {
              "ApiKey": "hello from secret",
              "Database": {
                "Password": "this is default db pass",
                "Port": 88
              },
              "ClientId": "68eb7465-2d66-43e9-8b58-32976ef0635f",
              "Feature": {
                "MaxRetries": 77,
                "Enabled": true
               }
            }


             */

            Console.WriteLine("========================================");
            Console.WriteLine("AppCfg.Net - User Secrets Demo");
            Console.WriteLine("========================================\n");

            try
            {
                // Check environment variables BEFORE initialization
                Console.WriteLine("--- Environment Variables Check ---");
                var demoValueEnv = Environment.GetEnvironmentVariable("APPCFG__DemoValue");
                if (demoValueEnv != null)
                {
                    Console.WriteLine($"✓ APPCFG__DemoValue is set to: \"{demoValueEnv}\"");
                }
                else
                {
                    Console.WriteLine("✗ APPCFG__DemoValue is NOT set");
                    Console.WriteLine("  To set it: $env:APPCFG__DemoValue = \"from-env\"");
                }
                Console.WriteLine();

                // Initialize settings - this loads secrets from the user secrets directory
                MySettings.Init();

                Console.WriteLine("User Secrets ID: " + MySettings.UserSecretsId);
                Console.WriteLine();

                // Display the secrets path
                var platform = Environment.OSVersion.Platform;
                string secretsPath;
                if (platform == PlatformID.Unix || platform == PlatformID.MacOSX)
                {
                    var home = Environment.GetEnvironmentVariable("HOME");
                    secretsPath = $"{home}/.microsoft/usersecrets/{MySettings.UserSecretsId}/secrets.json";
                }
                else
                {
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    secretsPath = $@"{appData}\Microsoft\UserSecrets\{MySettings.UserSecretsId}\secrets.json";
                }

                Console.WriteLine("Secrets file location:");
                Console.WriteLine(secretsPath);
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Loaded Settings:");
                Console.WriteLine("========================================\n");

                // Display loaded settings
                Console.WriteLine($"ApiKey: {MaskSecret(MySettings.SecretSettings.ApiKey)}");
                Console.WriteLine($"Database:Password: {MaskSecret(MySettings.SecretSettings.DatabasePassword)}");
                Console.WriteLine($"Database:Port: {MySettings.SecretSettings.DatabasePort}");
                Console.WriteLine($"ClientId: {MySettings.SecretSettings.ClientId}");
                Console.WriteLine($"Feature:MaxRetries: {MySettings.SecretSettings.MaxRetries}");
                Console.WriteLine($"Feature:Enabled: {MySettings.SecretSettings.FeatureEnabled}");
                Console.WriteLine();

                Console.WriteLine("--- Priority Chain Demo ---");
                Console.WriteLine($"DemoValue: {MySettings.SecretSettings.DemoValue}");
                Console.WriteLine("  (Checked: Env Var → Secrets → AppSettings → Default)");

                // Show where the value came from
                var envVal = Environment.GetEnvironmentVariable("APPCFG__DemoValue");
                Console.WriteLine();
                Console.WriteLine("  Source priority check:");
                if (envVal != null)
                {
                    Console.WriteLine($"    ✓ Environment Variable: \"{envVal}\" (HIGHEST PRIORITY - USED)");
                }
                else
                {
                    Console.WriteLine("    ✗ Environment Variable: not set");
                }
                Console.WriteLine("    ? User Secrets: check secrets.json");
                Console.WriteLine("    ? AppSettings: check App.config");

                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Demo completed successfully!");
                Console.WriteLine("========================================");

                /* 
                ## OUTPUT ##

                ApiKey: he****et
                Database:Password: th****ss
                Database:Port: 88
                ClientId: 68eb7465-2d66-43e9-8b58-32976ef0635f
                Feature:MaxRetries: 77
                Feature:Enabled: True

                 */
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("ERROR:");
                Console.WriteLine("========================================");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("Please see README.md for setup instructions.");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string MaskSecret(string secret)
        {
            if (string.IsNullOrEmpty(secret))
            {
                return "(not set - using default)";
            }

            if (secret.Length <= 4)
            {
                return "****";
            }

            return secret.Substring(0, 2) + "****" + secret.Substring(secret.Length - 2);
        }
    }
}
