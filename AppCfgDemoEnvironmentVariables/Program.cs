using System;

namespace AppCfgDemoEnvironmentVariables
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("AppCfg.Net - Environment Variables Demo");
            Console.WriteLine("========================================\n");

            try
            {
                // Initialize settings
                MySettings.Init();

                Console.WriteLine("This demo reads settings from environment variables.");
                Console.WriteLine("Environment variables use the format: APPCFG__Key__SubKey");
                Console.WriteLine("(Colons in property names become double underscores)\n");

                Console.WriteLine("========================================");
                Console.WriteLine("Environment Variables to Set:");
                Console.WriteLine("========================================");
                Console.WriteLine("APPCFG__ApiKey=your-api-key");
                Console.WriteLine("APPCFG__Database__Host=localhost");
                Console.WriteLine("APPCFG__Database__Port=5432");
                Console.WriteLine("APPCFG__Database__Password=your-password");
                Console.WriteLine("APPCFG__Feature__Enabled=true");
                Console.WriteLine("APPCFG__Feature__MaxRetries=5");
                Console.WriteLine("APPCFG__ClientId=12345678-1234-1234-1234-123456789abc");
                Console.WriteLine("APPCFG__SecretKey=my-secret");
                Console.WriteLine();

                Console.WriteLine("========================================");
                Console.WriteLine("Loaded Settings (Environment Variables):");
                Console.WriteLine("========================================\n");

                Console.WriteLine($"ApiKey: {MaskSecret(MySettings.EnvironmentSettings.ApiKey)}");
                Console.WriteLine($"Database:Host: {MySettings.EnvironmentSettings.DatabaseHost}");
                Console.WriteLine($"Database:Port: {MySettings.EnvironmentSettings.DatabasePort}");
                Console.WriteLine($"Database:Password: {MaskSecret(MySettings.EnvironmentSettings.DatabasePassword)}");
                Console.WriteLine($"Feature:Enabled: {MySettings.EnvironmentSettings.FeatureEnabled}");
                Console.WriteLine($"Feature:MaxRetries: {MySettings.EnvironmentSettings.MaxRetries}");
                Console.WriteLine($"ClientId: {MySettings.EnvironmentSettings.ClientId}");

                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Mixed Settings (Env Vars + AppSettings):");
                Console.WriteLine("========================================\n");

                Console.WriteLine($"SecretKey (from env): {MaskSecret(MySettings.MixedSettings.SecretKey)}");
                Console.WriteLine($"PublicSetting (from app.config): {MySettings.MixedSettings.PublicSetting}");

                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Note: DefaultOption Attribute");
                Console.WriteLine("========================================");
                Console.WriteLine("This demo uses [DefaultOption] on interfaces to avoid");
                Console.WriteLine("repeating StoreType and StoreIdentity on every property.");
                Console.WriteLine("See MySettings.cs for examples.");

                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("Demo completed successfully!");
                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("ERROR:");
                Console.WriteLine("========================================");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("Make sure to set the environment variables.");
                Console.WriteLine("See README.md for setup instructions.");
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
