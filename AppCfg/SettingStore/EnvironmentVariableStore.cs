using System;

namespace AppCfg.SettingStore
{
    /// <summary>
    /// Helper class for registering Environment Variables store.
    /// Reads settings from environment variables with hierarchical key support.
    /// Uses double underscore (__) to represent hierarchy (e.g., Database:Password → APPCFG__Database__Password)
    /// </summary>
    public static class EnvironmentVariableStore
    {
        /// <summary>
        /// Register an Environment Variables store with custom prefix.
        /// Profile key will be: "EnvironmentVariables:{prefix}"
        /// </summary>
        /// <param name="prefix">The prefix for environment variable names (e.g., "MYAPP__" or "APP_")</param>
        public static void Register(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentException("Prefix cannot be null or empty", nameof(prefix));
            }

            var profileKey = $"EnvironmentVariables:{prefix}";
            Register(prefix, profileKey);
        }

        /// <summary>
        /// Register an Environment Variables store with custom prefix and profile key.
        /// </summary>
        /// <param name="prefix">The prefix for environment variable names</param>
        /// <param name="profileKey">The custom profile key to use in [Option] attributes</param>
        public static void Register(string prefix, string profileKey)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentException("Prefix cannot be null or empty", nameof(prefix));
            }

            if (string.IsNullOrWhiteSpace(profileKey))
            {
                throw new ArgumentException("Profile key cannot be null or empty", nameof(profileKey));
            }

            MyAppCfg.SettingStores.RegisterStore(profileKey, metadata => GetValue(prefix, metadata.SettingKey));
        }

        /// <summary>
        /// Read a single setting from environment variables.
        /// "Database:Password" with prefix "APPCFG__" reads APPCFG__Database__Password.
        /// Returns null when the variable is not set (so default values apply).
        /// </summary>
        internal static string GetValue(string prefix, string settingKey)
        {
            try
            {
                var envVarName = ConvertKeyToEnvironmentVariable(prefix, settingKey);

                // Get the environment variable (case-insensitive on Windows, case-sensitive on Unix)
                var value = Environment.GetEnvironmentVariable(envVarName);

                if (value == null && !IsWindows())
                {
                    // On Unix systems, try case-insensitive search
                    value = GetEnvironmentVariableCaseInsensitive(envVarName);
                }

                return value;
            }
            catch (Exception ex)
            {
                throw new AppCfgException(
                    $"Error loading environment variable for key '{settingKey}' with prefix '{prefix}': {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Converts a setting key to environment variable format.
        /// Replaces colon (:) with double underscore (__) for hierarchical keys.
        /// </summary>
        /// <param name="prefix">The prefix (e.g., "APPCFG__")</param>
        /// <param name="key">The setting key (e.g., "Database:Password")</param>
        /// <returns>Environment variable name (e.g., "APPCFG__Database__Password")</returns>
        private static string ConvertKeyToEnvironmentVariable(string prefix, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            // Replace colon with double underscore (ASP.NET Core convention)
            var convertedKey = key.Replace(":", "__");

            return $"{prefix}{convertedKey}";
        }

        /// <summary>
        /// Case-insensitive environment variable lookup for Unix systems.
        /// </summary>
        private static string GetEnvironmentVariableCaseInsensitive(string name)
        {
            var variables = Environment.GetEnvironmentVariables();
            foreach (var key in variables.Keys)
            {
                if (string.Equals(key.ToString(), name, StringComparison.OrdinalIgnoreCase))
                {
                    return variables[key].ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if the current platform is Windows.
        /// </summary>
        private static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT ||
                   Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                   Environment.OSVersion.Platform == PlatformID.Win32S ||
                   Environment.OSVersion.Platform == PlatformID.WinCE;
        }
    }
}
