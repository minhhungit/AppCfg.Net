using System;
using System.Collections.Generic;

namespace AppCfg.SettingStore
{
    /// <summary>
    /// Priority-based configuration loading that checks multiple stores in order.
    /// Returns the first non-null value found.
    ///
    /// Priority order:
    /// 1. Environment Variables (highest priority)
    /// 2. User Secrets (if configured)
    /// 3. AppSettings (fallback)
    /// </summary>
    internal static class ChainedStore
    {
        /// <summary>
        /// Build a priority-based configuration chain.
        /// Returns a function that checks stores in order and returns the first non-null value.
        /// </summary>
        /// <param name="envVarPrefix">Environment variable prefix (default: "APPCFG__")</param>
        /// <param name="userSecretsId">User secrets ID (optional, if null, skips user secrets)</param>
        /// <returns>A function that takes a setting key and returns its value from the highest-priority source</returns>
        internal static Func<string, string> BuildChain(string envVarPrefix = "APPCFG__",
                                                         string userSecretsId = null)
        {
            // Build the chain of stores to check
            var chain = new List<Func<string, string>>();

            // 1. Environment Variables (highest priority)
            if (!string.IsNullOrEmpty(envVarPrefix))
            {
                chain.Add(settingKey => GetFromEnvironmentVariables(envVarPrefix, settingKey));
            }

            // 2. User Secrets (if configured)
            if (!string.IsNullOrEmpty(userSecretsId))
            {
                chain.Add(settingKey => GetFromUserSecrets(userSecretsId, settingKey));
            }

            // 3. AppSettings (lowest priority - fallback)
            chain.Add(settingKey => GetFromAppSettings(settingKey));

            // Return a function that executes the chain
            return settingKey =>
            {
                // Check each store in order, return first non-null value
                foreach (var getFunc in chain)
                {
                    var value = getFunc(settingKey);
                    if (value != null)
                    {
                        return value;
                    }
                }

                // No store had a value
                return null;
            };
        }

        /// <summary>
        /// Gets value from environment variables
        /// </summary>
        private static string GetFromEnvironmentVariables(string prefix, string settingKey)
        {
            try
            {
                var envVarName = $"{prefix}{settingKey.Replace(":", "__")}";
                var value = Environment.GetEnvironmentVariable(envVarName);

                // On Unix, try case-insensitive if not found
                if (value == null && (Environment.OSVersion.Platform == PlatformID.Unix ||
                                     Environment.OSVersion.Platform == PlatformID.MacOSX))
                {
                    var variables = Environment.GetEnvironmentVariables();
                    foreach (var key in variables.Keys)
                    {
                        if (string.Equals(key.ToString(), envVarName, StringComparison.OrdinalIgnoreCase))
                        {
                            return variables[key].ToString();
                        }
                    }
                }

                return value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets value from user secrets - reads directly from secrets file
        /// </summary>
        private static string GetFromUserSecrets(string userSecretsId, string settingKey)
        {
            try
            {
                // Auto-register UserSecretsStore if not already registered
                var profileKey = $"UserSecrets:{userSecretsId}";

                // Check if already registered
                var existingStore = MyAppCfg.SettingStores.Get(profileKey);
                if (existingStore == null)
                {
                    // Register it automatically
                    UserSecretsStore.Register(userSecretsId, profileKey);
                }

                // Now get the value
                var store = MyAppCfg.SettingStores.Get(profileKey);
                if (store is Func<MyAppCfg.SettingStoreMetadata, string> func)
                {
                    var metadata = new MyAppCfg.SettingStoreMetadata(profileKey, null, settingKey, typeof(string));
                    return func(metadata);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets value from app settings or connection strings
        /// </summary>
        private static string GetFromAppSettings(string settingKey)
        {
            try
            {
                // First try AppSettings
                var value = System.Configuration.ConfigurationManager.AppSettings[settingKey];
                if (value != null)
                {
                    return value;
                }

                // Then try ConnectionStrings
                var connString = System.Configuration.ConfigurationManager.ConnectionStrings[settingKey];
                if (connString != null)
                {
                    return connString.ConnectionString;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
