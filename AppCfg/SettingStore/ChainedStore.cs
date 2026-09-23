using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AppCfg.SettingStore
{
    /// <summary>
    /// Priority-based configuration loading that checks multiple stores in order.
    /// Returns the first non-null value found.
    ///
    /// Priority order:
    /// 1. Environment Variables (highest priority)
    /// 2. User Secrets (if configured)
    /// 3. AppSettings / ConnectionStrings (fallback)
    /// </summary>
    internal static class ChainedStore
    {
        /// <summary>
        /// Build a priority-based configuration chain.
        /// Returns a function that checks stores in order and returns the first non-null value.
        /// </summary>
        /// <param name="envVarPrefix">Environment variable prefix (default: "APPCFG__")</param>
        /// <param name="userSecretsId">User secrets ID (optional, if null, skips user secrets)</param>
        /// <returns>A function that takes a setting key and the setting's type and returns its value from the highest-priority source</returns>
        internal static Func<string, Type, string> BuildChain(string envVarPrefix = "APPCFG__",
                                                               string userSecretsId = null)
        {
            var chain = new List<Func<string, Type, string>>();

            // 1. Environment Variables (highest priority)
            if (!string.IsNullOrEmpty(envVarPrefix))
            {
                chain.Add((settingKey, _) => EnvironmentVariableStore.GetValue(envVarPrefix, settingKey));
            }

            // 2. User Secrets (if configured). Errors in secrets.json (bad JSON, unreadable file)
            //    are deliberately NOT swallowed here: a broken secrets file must surface, not silently
            //    fall through to App.config.
            if (!string.IsNullOrEmpty(userSecretsId))
            {
                chain.Add((settingKey, _) => UserSecretsStore.GetValue(userSecretsId, settingKey));
            }

            // 3. AppSettings / ConnectionStrings (lowest priority - fallback)
            chain.Add(GetFromAppConfig);

            return (settingKey, typeOfSetting) =>
            {
                foreach (var getFunc in chain)
                {
                    var value = getFunc(settingKey, typeOfSetting);
                    if (value != null)
                    {
                        return value;
                    }
                }

                return null;
            };
        }

        /// <summary>
        /// Gets value from app settings or connection strings.
        /// For <see cref="SqlConnectionStringBuilder"/> settings the connectionStrings section is checked first,
        /// matching the behaviour of the non-chained App.config store.
        /// </summary>
        private static string GetFromAppConfig(string settingKey, Type typeOfSetting)
        {
            try
            {
                if (typeOfSetting == typeof(SqlConnectionStringBuilder))
                {
                    return GetConnectionString(settingKey)
                        ?? System.Configuration.ConfigurationManager.AppSettings[settingKey];
                }

                return System.Configuration.ConfigurationManager.AppSettings[settingKey]
                    ?? GetConnectionString(settingKey);
            }
            catch
            {
                // No config file / unreadable config: treat as "no value" so defaults apply
                return null;
            }
        }

        private static string GetConnectionString(string settingKey)
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings[settingKey]?.ConnectionString;
        }
    }
}
