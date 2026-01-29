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
    /// Reads secrets from JSON files in the user's profile directory.
    /// </summary>
    public static class UserSecretsStore
    {
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _secretsCache
            = new ConcurrentDictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Register a User Secrets store with default store identity pattern.
        /// Store identity will be: "UserSecrets:{userSecretsId}"
        /// </summary>
        /// <param name="userSecretsId">The user secrets ID (directory name under UserSecrets folder)</param>
        public static void Register(string userSecretsId)
        {
            if (string.IsNullOrWhiteSpace(userSecretsId))
            {
                throw new ArgumentException("User secrets ID cannot be null or empty", nameof(userSecretsId));
            }

            var storeIdentity = $"UserSecrets:{userSecretsId}";
            Register(userSecretsId, storeIdentity);
        }

        /// <summary>
        /// Register a User Secrets store with custom store identity.
        /// </summary>
        /// <param name="userSecretsId">The user secrets ID (directory name under UserSecrets folder)</param>
        /// <param name="storeIdentity">The custom store identity to use in [Option] attributes</param>
        public static void Register(string userSecretsId, string storeIdentity)
        {
            if (string.IsNullOrWhiteSpace(userSecretsId))
            {
                throw new ArgumentException("User secrets ID cannot be null or empty", nameof(userSecretsId));
            }

            if (string.IsNullOrWhiteSpace(storeIdentity))
            {
                throw new ArgumentException("Store identity cannot be null or empty", nameof(storeIdentity));
            }

            MyAppCfg.SettingStores.RegisterCustomStore(storeIdentity, metadata =>
            {
                try
                {
                    // Load or get cached secrets for this userSecretsId
                    var secrets = _secretsCache.GetOrAdd(userSecretsId, id =>
                    {
                        var path = GetUserSecretsPath(id);
                        return LoadSecrets(path);
                    });

                    // Try to get the value by key
                    if (secrets != null && secrets.TryGetValue(metadata.SettingKey, out var value))
                    {
                        return value;
                    }

                    // Return null to allow default values to work
                    return null;
                }
                catch (Exception ex)
                {
                    throw new AppCfgException(
                        $"Error loading secret '{metadata.SettingKey}' from User Secrets (ID: {userSecretsId}): {ex.Message}",
                        ex);
                }
            });
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
        /// </summary>
        private static string GetUserSecretsPath(string userSecretsId)
        {
            string basePath;

            // Determine platform-specific base path
            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                // Linux/macOS: ~/.microsoft/usersecrets/{id}/secrets.json
                var home = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrWhiteSpace(home))
                {
                    throw new AppCfgException("HOME environment variable is not set");
                }
                basePath = Path.Combine(home, ".microsoft", "usersecrets");
            }
            else
            {
                // Windows: %APPDATA%\Microsoft\UserSecrets\{id}\secrets.json
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrWhiteSpace(appData))
                {
                    throw new AppCfgException("ApplicationData folder path could not be determined");
                }
                basePath = Path.Combine(appData, "Microsoft", "UserSecrets");
            }

            var secretsPath = Path.Combine(basePath, userSecretsId, "secrets.json");
            return secretsPath;
        }

        /// <summary>
        /// Load secrets from the JSON file at the specified path.
        /// Supports both flat keys and hierarchical keys with colon notation (e.g., "Database:Password").
        /// </summary>
        private static Dictionary<string, string> LoadSecrets(string path)
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
        /// Recursively flatten a JObject into a dictionary with colon-separated keys.
        /// Example: {"Database": {"Password": "test"}} becomes {"Database:Password": "test"}
        /// </summary>
        private static void FlattenJson(JObject jObject, Dictionary<string, string> result, string prefix)
        {
            foreach (var property in jObject.Properties())
            {
                var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";

                if (property.Value is JObject nestedObject)
                {
                    // Recursively flatten nested objects
                    FlattenJson(nestedObject, result, key);
                }
                else if (property.Value is JArray)
                {
                    // Arrays are not supported in this simple implementation
                    // Could be extended to support array indexing (e.g., "Array:0", "Array:1")
                    throw new AppCfgException($"Array values are not supported in User Secrets. Key: {key}");
                }
                else
                {
                    // Store the value as a string
                    result[key] = property.Value.ToString();
                }
            }
        }
    }
}
