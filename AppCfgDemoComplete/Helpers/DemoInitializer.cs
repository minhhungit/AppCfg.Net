using System;
using System.IO;

namespace AppCfgDemoComplete.Helpers
{
    /// <summary>
    /// Initializes demo environment variables and secrets.json file for demonstration purposes.
    /// This allows the demo to work out-of-the-box without manual configuration.
    /// </summary>
    public static class DemoInitializer
    {
        private const string UserSecretsId = "appcfg-demo-complete";
        private const string EnvVarPrefix = "APPCFG__";

        /// <summary>
        /// Initialize all demo resources (environment variables and secrets file).
        /// </summary>
        public static void Initialize()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              AppCfg.Net Demo - Environment Setup              ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            InitializeEnvironmentVariables();
            InitializeSecretsFile();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Demo environment ready!");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Set up demo environment variables for the current process.
        /// </summary>
        private static void InitializeEnvironmentVariables()
        {
            Console.WriteLine("Setting up environment variables for this session:");

            // Environment Variables Demo
            SetEnvVar("Env__SimpleString", "from-environment-variable");
            SetEnvVar("Env__IntValue", "999");

            // Chained Store Demo - Environment variables have highest priority
            SetEnvVar("ChainedDemo__EnvVarValue", "value-from-ENV-VAR");
            SetEnvVar("ChainedDemo__OverrideMe", "OVERRIDDEN-BY-ENV-VAR");

            // Computed Settings Demo - Optional overrides
            SetEnvVar("Computed__Host", "env-db-server.example.com");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Environment variables initialized for current process.\n");
            Console.ResetColor();
        }

        /// <summary>
        /// Set an environment variable with the APPCFG__ prefix.
        /// </summary>
        private static void SetEnvVar(string key, string value)
        {
            var fullKey = EnvVarPrefix + key;
            Environment.SetEnvironmentVariable(fullKey, value);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  SET {fullKey}={value}");
            Console.ResetColor();
        }

        /// <summary>
        /// Initialize the secrets.json file with demo values.
        /// Warns if the file already exists.
        /// </summary>
        private static void InitializeSecretsFile()
        {
            var secretsPath = GetSecretsFilePath();
            var secretsDir = Path.GetDirectoryName(secretsPath);

            Console.WriteLine("Setting up secrets.json file:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Path: {secretsPath}");
            Console.ResetColor();

            if (File.Exists(secretsPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  WARNING: secrets.json already exists - skipping creation.");
                Console.WriteLine("  Delete the file manually if you want to regenerate demo secrets.");
                Console.ResetColor();
                return;
            }

            try
            {
                // Create directory if it doesn't exist
                if (!Directory.Exists(secretsDir))
                {
                    Directory.CreateDirectory(secretsDir);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  Created directory: {secretsDir}");
                    Console.ResetColor();
                }

                // Create secrets.json with demo values
                var secretsContent = @"{
  ""Secret:ApiKey"": ""demo-api-key-abc123xyz789"",
  ""Secret:DatabasePassword"": ""DemoP@ssw0rd!2024"",
  ""Secret:EncryptionKey"": ""demo-encryption-key-secure-value"",
  ""ChainedDemo:SecretValue"": ""value-from-USER-SECRETS"",
  ""ChainedDemo:OverrideMe"": ""overridden-by-secrets""
}";

                File.WriteAllText(secretsPath, secretsContent);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  Created secrets.json with demo values.");
                Console.ResetColor();

                // Show what was created
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Contents:");
                Console.WriteLine("    - Secret:ApiKey");
                Console.WriteLine("    - Secret:DatabasePassword");
                Console.WriteLine("    - Secret:EncryptionKey");
                Console.WriteLine("    - ChainedDemo:SecretValue");
                Console.WriteLine("    - ChainedDemo:OverrideMe");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ERROR: Failed to create secrets.json: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Get the full path to the secrets.json file.
        /// </summary>
        private static string GetSecretsFilePath()
        {
            string basePath;

            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                // Linux/macOS: ~/.microsoft/usersecrets/{id}/secrets.json
                var home = Environment.GetEnvironmentVariable("HOME");
                basePath = Path.Combine(home ?? "~", ".microsoft", "usersecrets");
            }
            else
            {
                // Windows: %APPDATA%\Microsoft\UserSecrets\{id}\secrets.json
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                basePath = Path.Combine(appData, "Microsoft", "UserSecrets");
            }

            return Path.Combine(basePath, UserSecretsId, "secrets.json");
        }

        /// <summary>
        /// Clean up demo resources (optional - for testing).
        /// </summary>
        public static void Cleanup()
        {
            // Clear environment variables
            Environment.SetEnvironmentVariable(EnvVarPrefix + "Env__SimpleString", null);
            Environment.SetEnvironmentVariable(EnvVarPrefix + "Env__IntValue", null);
            Environment.SetEnvironmentVariable(EnvVarPrefix + "ChainedDemo__EnvVarValue", null);
            Environment.SetEnvironmentVariable(EnvVarPrefix + "ChainedDemo__OverrideMe", null);
            Environment.SetEnvironmentVariable(EnvVarPrefix + "Computed__Host", null);

            Console.WriteLine("Demo environment variables cleared.");
        }

        /// <summary>
        /// Delete the demo secrets.json file (optional - for testing).
        /// </summary>
        public static void DeleteSecretsFile()
        {
            var secretsPath = GetSecretsFilePath();
            if (File.Exists(secretsPath))
            {
                File.Delete(secretsPath);
                Console.WriteLine($"Deleted: {secretsPath}");
            }
        }
    }
}
