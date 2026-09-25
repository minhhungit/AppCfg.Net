using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppCfg.SettingStore
{
    /// <summary>
    /// Helper class for registering User Secrets store (similar to .NET Core Secret Manager).
    /// Reads secrets from JSON files in the user's profile directory, or under the folder named by
    /// the <see cref="RootEnvironmentVariable"/> environment variable when it is set.
    /// </summary>
    public static class UserSecretsStore
    {
        /// <summary>
        /// Environment variable that overrides the folder holding the per-ID secrets folders.
        /// When set, secrets are read from {root}\{userSecretsId}\secrets.json instead of the user profile,
        /// e.g. for an IIS application pool that runs without a loaded user profile.
        /// </summary>
        public const string RootEnvironmentVariable = "APPCFG_USERSECRETS_ROOT";

        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _secretsCache
            = new ConcurrentDictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Register a User Secrets store with default profile key pattern.
        /// Profile key will be: "UserSecrets:{userSecretsId}"
        /// </summary>
        /// <param name="userSecretsId">The user secrets ID (directory name under UserSecrets folder)</param>
        public static void Register(string userSecretsId)
        {
            if (string.IsNullOrWhiteSpace(userSecretsId))
            {
                throw new ArgumentException("User secrets ID cannot be null or empty", nameof(userSecretsId));
            }

            var profileKey = $"UserSecrets:{userSecretsId}";
            Register(userSecretsId, profileKey);
        }

        /// <summary>
        /// Register a User Secrets store with custom profile key.
        /// </summary>
        /// <param name="userSecretsId">The user secrets ID (directory name under UserSecrets folder)</param>
        /// <param name="profileKey">The custom profile key to use in [Option] attributes</param>
        public static void Register(string userSecretsId, string profileKey)
        {
            if (string.IsNullOrWhiteSpace(userSecretsId))
            {
                throw new ArgumentException("User secrets ID cannot be null or empty", nameof(userSecretsId));
            }

            if (string.IsNullOrWhiteSpace(profileKey))
            {
                throw new ArgumentException("Profile key cannot be null or empty", nameof(profileKey));
            }

            MyAppCfg.SettingStores.RegisterStore(profileKey, metadata => GetValue(userSecretsId, metadata.SettingKey));
        }

        /// <summary>
        /// Read a single value from the secrets.json of the given user secrets ID.
        /// Returns null when the file or the key does not exist (so default values apply).
        /// Throws <see cref="AppCfgException"/> when the file exists but cannot be read or parsed.
        /// </summary>
        internal static string GetValue(string userSecretsId, string settingKey)
        {
            try
            {
                // Load or get cached secrets for this userSecretsId
                // No secrets location (no root override and no user profile) means no secrets, like a missing file
                var secrets = _secretsCache.GetOrAdd(userSecretsId, id =>
                {
                    var path = ResolveSecretsPath(id);
                    return path == null
                        ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        : LoadSecrets(path);
                });

                if (secrets != null && secrets.TryGetValue(settingKey, out var value))
                {
                    return value;
                }

                // Return null to allow default values to work
                return null;
            }
            catch (Exception ex)
            {
                throw new AppCfgException(
                    $"Error loading secret '{settingKey}' from User Secrets (ID: {userSecretsId}): {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Clear the secrets cache. Useful for testing or when secrets have been updated.
        /// </summary>
        public static void ClearCache()
        {
            _secretsCache.Clear();
        }

        /// <summary>
        /// Clear the cache for a specific user secrets ID.
        /// </summary>
        /// <param name="userSecretsId">The user secrets ID to clear from cache</param>
        public static void ClearCache(string userSecretsId)
        {
            if (!string.IsNullOrWhiteSpace(userSecretsId))
            {
                _secretsCache.TryRemove(userSecretsId, out _);
            }
        }

        /// <summary>
        /// Get the full path to the secrets.json file for the given user secrets ID.
        /// When the <see cref="RootEnvironmentVariable"/> environment variable is set: {root}\{id}\secrets.json.
        /// Otherwise the .NET Core Secret Manager convention:
        /// Windows: %APPDATA%\Microsoft\UserSecrets\{id}\secrets.json,
        /// Linux/macOS: ~/.microsoft/usersecrets/{id}/secrets.json
        /// Throws <see cref="AppCfgException"/> when no location can be determined.
        /// </summary>
        public static string GetSecretsFilePath(string userSecretsId)
        {
            if (string.IsNullOrWhiteSpace(userSecretsId))
            {
                throw new ArgumentException("User secrets ID cannot be null or empty", nameof(userSecretsId));
            }

            return GetUserSecretsPath(userSecretsId);
        }

        private static string GetUserSecretsPath(string userSecretsId)
        {
            var secretsPath = ResolveSecretsPath(userSecretsId);
            if (secretsPath == null)
            {
                throw new AppCfgException(IsUnix()
                    ? $"HOME environment variable is not set and {RootEnvironmentVariable} is not set"
                    : $"ApplicationData folder path could not be determined and {RootEnvironmentVariable} is not set");
            }

            return secretsPath;
        }

        /// <summary>
        /// Resolve the secrets.json path for the given ID, or null when there is no root override
        /// and no user profile (e.g. an IIS application pool with Load User Profile off).
        /// </summary>
        private static string ResolveSecretsPath(string userSecretsId)
        {
            var basePath = Environment.GetEnvironmentVariable(RootEnvironmentVariable);

            if (string.IsNullOrWhiteSpace(basePath))
            {
                // Determine platform-specific base path
                if (IsUnix())
                {
                    // Linux/macOS: ~/.microsoft/usersecrets/{id}/secrets.json
                    var home = Environment.GetEnvironmentVariable("HOME");
                    if (string.IsNullOrWhiteSpace(home))
                    {
                        return null;
                    }
                    basePath = Path.Combine(home, ".microsoft", "usersecrets");
                }
                else
                {
                    // Windows: %APPDATA%\Microsoft\UserSecrets\{id}\secrets.json
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    if (string.IsNullOrWhiteSpace(appData))
                    {
                        return null;
                    }
                    basePath = Path.Combine(appData, "Microsoft", "UserSecrets");
                }
            }

            return Path.Combine(basePath, userSecretsId, "secrets.json");
        }

        private static bool IsUnix()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX;
        }

        /// <summary>
        /// Load secrets from the JSON file at the specified path.
        /// Supports both flat keys and hierarchical keys with colon notation (e.g., "Database:Password").
        /// </summary>
        internal static Dictionary<string, string> LoadSecrets(string path)
        {
            if (!File.Exists(path))
            {
                // Return empty dictionary if file doesn't exist
                // This allows default values to be used
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                var json = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                // Parse JSON and flatten to key-value pairs
                var secrets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var jObject = JObject.Parse(json);

                FlattenJson(jObject, secrets, string.Empty);

                return secrets;
            }
            catch (JsonException ex)
            {
                throw new AppCfgException($"Invalid JSON in secrets file: {path}. Error: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new AppCfgException($"Error reading secrets file: {path}. Error: {ex.Message}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new AppCfgException($"Access denied to secrets file: {path}. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Recursively flatten a JSON token into a dictionary with colon-separated keys,
        /// following the same rules as the .NET Core JSON configuration provider:
        /// objects become "Parent:Child", arrays become "Parent:0", "Parent:1", ...
        /// and null values are skipped (treated as missing so default values apply).
        /// Example: {"Database": {"Password": "test"}} becomes {"Database:Password": "test"}
        /// </summary>
        internal static void FlattenJson(JToken token, IDictionary<string, string> result, string prefix)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (var property in obj.Properties())
                    {
                        FlattenJson(property.Value, result, Combine(prefix, property.Name));
                    }
                    break;

                case JArray array:
                    for (var i = 0; i < array.Count; i++)
                    {
                        FlattenJson(array[i], result, Combine(prefix, i.ToString()));
                    }
                    break;

                case JValue value:
                    if (value.Type != JTokenType.Null && value.Type != JTokenType.Undefined)
                    {
                        result[prefix] = value.ToString();
                    }
                    break;
            }
        }

        private static string Combine(string prefix, string name)
        {
            return string.IsNullOrEmpty(prefix) ? name : $"{prefix}:{name}";
        }
    }
}
