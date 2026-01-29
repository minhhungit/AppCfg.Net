using System;
using System.Collections.Generic;

namespace AppCfg.SettingStore
{
    /// <summary>
    /// A chained store that checks multiple stores in priority order.
    /// Returns the first non-null value found.
    ///
    /// Typical priority order:
    /// 1. Environment Variables (highest priority)
    /// 2. User Secrets
    /// 3. AppSettings/Database/Redis (lowest priority)
    /// </summary>
    public static class ChainedStore
    {
        /// <summary>
        /// Register a chained store with default priority order:
        /// 1. Environment Variables (prefix: envVarPrefix)
        /// 2. User Secrets (if userSecretsId provided)
        /// 3. AppSettings
        /// </summary>
        /// <param name="storeIdentity">The identity for this chained store (e.g., "ChainedStore:Default")</param>
        /// <param name="envVarPrefix">Environment variable prefix (default: "APPCFG__")</param>
        /// <param name="userSecretsId">User secrets ID (optional, if null, skips user secrets)</param>
        public static void Register(string storeIdentity = "ChainedStore:Default",
                                     string envVarPrefix = "APPCFG__",
                                     string userSecretsId = null)
        {
            if (string.IsNullOrWhiteSpace(storeIdentity))
            {
                throw new ArgumentException("Store identity cannot be null or empty", nameof(storeIdentity));
            }

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

            // Register the chained store
            MyAppCfg.SettingStores.RegisterCustomStore(storeIdentity, metadata =>
            {
                // Check each store in order, return first non-null value
                foreach (var getFunc in chain)
                {
                    var value = getFunc(metadata.SettingKey);
                    if (value != null)
                    {
                        return value;
                    }
                }

                // No store had a value
                return null;
            });
        }

        /// <summary>
        /// Register a chained store with custom store chain.
        /// Stores are checked in the order provided.
        /// </summary>
        /// <param name="storeIdentity">The identity for this chained store</param>
        /// <param name="storeChain">List of (storeIdentity, storeType) to check in order</param>
        public static void RegisterCustomChain(string storeIdentity, params (string identity, StoreType type)[] storeChain)
        {
            if (string.IsNullOrWhiteSpace(storeIdentity))
            {
                throw new ArgumentException("Store identity cannot be null or empty", nameof(storeIdentity));
            }

            if (storeChain == null || storeChain.Length == 0)
            {
                throw new ArgumentException("Store chain cannot be empty", nameof(storeChain));
            }

            MyAppCfg.SettingStores.RegisterCustomStore(storeIdentity, metadata =>
            {
                // Check each store in the chain
                foreach (var (identity, type) in storeChain)
                {
                    string value = null;

                    switch (type)
                    {
                        case StoreType.EnvironmentVariables:
                            value = GetFromEnvironmentVariables(identity, metadata.SettingKey);
                            break;
                        case StoreType.UserSecrets:
                            value = GetFromUserSecrets(identity, metadata.SettingKey);
                            break;
                        case StoreType.AppSettings:
                            value = GetFromAppSettings(metadata.SettingKey);
                            break;
                        case StoreType.CustomStore:
                            // Delegate to another custom store
                            var customStore = MyAppCfg.SettingStores.Get(SettingStoreType.Custom, identity);
                            if (customStore is Func<MyAppCfg.SettingStoreMetadata, string> func)
                            {
                                value = func(metadata);
                            }
                            break;
                    }

                    if (value != null)
                    {
                        return value;
                    }
                }

                return null;
            });
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
                var storeIdentity = $"UserSecrets:{userSecretsId}";

                // Check if already registered
                var existingStore = MyAppCfg.SettingStores.Get(SettingStoreType.Custom, storeIdentity);
                if (existingStore == null)
                {
                    // Register it automatically
                    UserSecretsStore.Register(userSecretsId, storeIdentity);
                }

                // Now get the value
                var store = MyAppCfg.SettingStores.Get(SettingStoreType.Custom, storeIdentity);
                if (store is Func<MyAppCfg.SettingStoreMetadata, string> func)
                {
                    var metadata = new MyAppCfg.SettingStoreMetadata(storeIdentity, null, settingKey, typeof(string));
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
        /// Gets value from app settings
        /// </summary>
        private static string GetFromAppSettings(string settingKey)
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings[settingKey];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Store types for custom chain configuration
        /// </summary>
        public enum StoreType
        {
            EnvironmentVariables,
            UserSecrets,
            AppSettings,
            CustomStore
        }
    }
}
