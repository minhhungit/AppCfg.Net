using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace AppCfg.SettingStore
{
    /// <summary>
    /// Moves appSettings / connectionStrings from a .NET Framework App.config or Web.config into the
    /// user-secrets file that <see cref="UserSecretsStore"/> and <see cref="MyAppCfg.Configure(string, string)"/> read.
    ///
    /// Typical one-off use during development:
    /// <code>
    /// UserSecretsMigrator.MigrateFromCurrentConfig("my-app-secrets",
    ///     new MigrationOptions { KeyFilter = k => k.Contains("Password") || k.EndsWith("Key") });
    /// </code>
    /// Afterwards delete the migrated keys from App.config and call
    /// <c>MyAppCfg.Configure(userSecretsId: "my-app-secrets")</c> at startup.
    /// </summary>
    public static class UserSecretsMigrator
    {
        /// <summary>
        /// Migrate the appSettings (and, by default, connectionStrings) of an App.config / Web.config file on disk.
        /// Honours <c>file="..."</c> on appSettings and <c>configSource="..."</c> on both sections.
        /// </summary>
        /// <param name="configFilePath">Path to App.config, Web.config, MyApp.exe.config, ...</param>
        /// <param name="userSecretsId">User secrets ID (directory name under the UserSecrets folder)</param>
        /// <param name="options">Optional filtering / overwrite behaviour</param>
        public static MigrationResult MigrateFromConfigFile(string configFilePath, string userSecretsId, MigrationOptions options = null)
        {
            options = options ?? new MigrationOptions();
            var settings = ReadConfigFile(configFilePath, options.IncludeConnectionStrings);
            return Migrate(settings, userSecretsId, options);
        }

        /// <summary>
        /// Migrate a .NET Core style JSON file (appsettings.json, appsettings.Development.json, ...).
        /// Nested objects become "Parent:Child" keys and arrays become "Parent:0", "Parent:1",
        /// exactly as the .NET Core JSON provider and <see cref="UserSecretsStore"/> read them.
        /// </summary>
        public static MigrationResult MigrateFromJsonFile(string jsonFilePath, string userSecretsId, MigrationOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(jsonFilePath))
            {
                throw new ArgumentException("JSON file path cannot be null or empty", nameof(jsonFilePath));
            }

            if (!File.Exists(jsonFilePath))
            {
                throw new AppCfgException($"JSON file not found: {jsonFilePath}");
            }

            // LoadSecrets already implements the .NET Core flattening rules and error reporting
            var settings = UserSecretsStore.LoadSecrets(jsonFilePath);
            return Migrate(settings, userSecretsId, options);
        }

        /// <summary>
        /// Migrate the configuration of the running application (whatever <see cref="ConfigurationManager"/> sees).
        /// Connection strings inherited from machine.config (e.g. LocalSqlServer) are ignored.
        /// </summary>
        public static MigrationResult MigrateFromCurrentConfig(string userSecretsId, MigrationOptions options = null)
        {
            options = options ?? new MigrationOptions();
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (key != null)
                {
                    settings[key] = ConfigurationManager.AppSettings[key] ?? string.Empty;
                }
            }

            if (options.IncludeConnectionStrings)
            {
                foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
                {
                    if (IsInheritedFromMachineConfig(cs) || settings.ContainsKey(cs.Name))
                    {
                        continue;
                    }

                    settings[cs.Name] = cs.ConnectionString ?? string.Empty;
                }
            }

            return Migrate(settings, userSecretsId, options);
        }

        /// <summary>
        /// Write the given key/values into the secrets.json of <paramref name="userSecretsId"/>,
        /// merging with whatever is already there. This is the core the other overloads build on and
        /// can be used with settings gathered from any source.
        /// </summary>
        public static MigrationResult Migrate(IDictionary<string, string> settings, string userSecretsId, MigrationOptions options = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            options = options ?? new MigrationOptions();
            var secretsPath = UserSecretsStore.GetSecretsFilePath(userSecretsId);

            // Existing secrets first (flattened to "A:B" keys), so unrelated secrets survive the merge
            var order = new List<string>();
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var existing in UserSecretsStore.LoadSecrets(secretsPath))
            {
                order.Add(existing.Key);
                values[existing.Key] = existing.Value;
            }

            var migrated = new List<string>();
            var skipped = new List<string>();

            foreach (var entry in settings)
            {
                if (options.KeyFilter != null && !options.KeyFilter(entry.Key))
                {
                    continue;
                }

                if (values.ContainsKey(entry.Key))
                {
                    if (!options.OverwriteExisting)
                    {
                        skipped.Add(entry.Key);
                        continue;
                    }
                }
                else
                {
                    order.Add(entry.Key);
                }

                values[entry.Key] = entry.Value ?? string.Empty;
                migrated.Add(entry.Key);
            }

            if (options.DryRun)
            {
                return new MigrationResult(secretsPath, migrated, skipped);
            }

            var output = new JObject();
            foreach (var key in order)
            {
                output[key] = values[key];
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(secretsPath));
                File.WriteAllText(secretsPath, output.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new AppCfgException($"Could not write secrets file: {secretsPath}. Error: {ex.Message}", ex);
            }

            // Make sure the next read sees the new content
            UserSecretsStore.ClearCache(userSecretsId);

            return new MigrationResult(secretsPath, migrated, skipped);
        }

        /// <summary>
        /// Parse an App.config / Web.config file into a flat key → value dictionary without writing anything.
        /// Useful to preview a migration. Honours add/remove/clear, <c>file="..."</c> on appSettings and
        /// <c>configSource="..."</c> on both sections. Connection strings are keyed by their name;
        /// an appSettings key with the same name wins.
        /// </summary>
        public static IDictionary<string, string> ReadConfigFile(string configFilePath, bool includeConnectionStrings = true)
        {
            if (string.IsNullOrWhiteSpace(configFilePath))
            {
                throw new ArgumentException("Config file path cannot be null or empty", nameof(configFilePath));
            }

            if (!File.Exists(configFilePath))
            {
                throw new AppCfgException($"Config file not found: {configFilePath}");
            }

            var baseDir = Path.GetDirectoryName(Path.GetFullPath(configFilePath));
            var root = LoadXml(configFilePath).Root;
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Accept a full <configuration> file as well as a standalone section file
            // (root <appSettings> or <connectionStrings>, as referenced by file= / configSource=).
            XElement appSettings = null, connectionStrings = null;
            switch (root?.Name.LocalName)
            {
                case "appSettings":
                    appSettings = root;
                    break;
                case "connectionStrings":
                    connectionStrings = root;
                    break;
                default:
                    appSettings = root?.Element("appSettings");
                    connectionStrings = root?.Element("connectionStrings");
                    break;
            }

            if (appSettings != null)
            {
                var section = ResolveConfigSource(appSettings, baseDir);
                ApplyAddRemoveClear(section, result, "key", "value");

                // <appSettings file="external.config"> - external entries add to / override the inline ones
                var externalFile = appSettings.Attribute("file")?.Value;
                if (!string.IsNullOrWhiteSpace(externalFile))
                {
                    var externalPath = Path.Combine(baseDir, externalFile);
                    if (File.Exists(externalPath)) // .NET silently ignores a missing file= target
                    {
                        ApplyAddRemoveClear(LoadXml(externalPath).Root, result, "key", "value");
                    }
                }
            }

            if (includeConnectionStrings && connectionStrings != null)
            {
                var section = ResolveConfigSource(connectionStrings, baseDir);
                var conns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                ApplyAddRemoveClear(section, conns, "name", "connectionString");

                foreach (var conn in conns)
                {
                    if (!result.ContainsKey(conn.Key))
                    {
                        result[conn.Key] = conn.Value;
                    }
                }
            }

            return result;
        }

        private static XDocument LoadXml(string path)
        {
            try
            {
                return XDocument.Load(path);
            }
            catch (XmlException ex)
            {
                throw new AppCfgException($"Invalid XML in config file: {path}. Error: {ex.Message}", ex);
            }
        }

        private static XElement ResolveConfigSource(XElement section, string baseDir)
        {
            var configSource = section.Attribute("configSource")?.Value;
            if (string.IsNullOrWhiteSpace(configSource))
            {
                return section;
            }

            var path = Path.Combine(baseDir, configSource);
            if (!File.Exists(path))
            {
                throw new AppCfgException($"configSource file for <{section.Name}> not found: {path}");
            }

            return LoadXml(path).Root;
        }

        private static void ApplyAddRemoveClear(XElement section, IDictionary<string, string> target, string keyAttribute, string valueAttribute)
        {
            if (section == null)
            {
                return;
            }

            foreach (var element in section.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "add":
                        var key = element.Attribute(keyAttribute)?.Value;
                        if (!string.IsNullOrEmpty(key))
                        {
                            target[key] = element.Attribute(valueAttribute)?.Value ?? string.Empty;
                        }
                        break;

                    case "remove":
                        var removeKey = element.Attribute(keyAttribute)?.Value;
                        if (!string.IsNullOrEmpty(removeKey))
                        {
                            target.Remove(removeKey);
                        }
                        break;

                    case "clear":
                        target.Clear();
                        break;
                }
            }
        }

        /// <summary>
        /// Entries declared in the application's own config file report that file as their
        /// ElementInformation.Source. Entries inherited from machine.config (LocalSqlServer) report
        /// machine.config, or no source at all depending on the host, so both are treated as inherited.
        /// </summary>
        private static bool IsInheritedFromMachineConfig(ConnectionStringSettings cs)
        {
            try
            {
                var source = cs.ElementInformation?.Source;
                return string.IsNullOrEmpty(source)
                       || source.EndsWith("machine.config", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Options for <see cref="UserSecretsMigrator"/>.
    /// </summary>
    public class MigrationOptions
    {
        /// <summary>Also migrate connectionStrings entries, keyed by their name. Default: true.</summary>
        public bool IncludeConnectionStrings { get; set; } = true;

        /// <summary>
        /// Replace keys that already exist in secrets.json. Default: false (existing local secrets win
        /// and are reported in <see cref="MigrationResult.SkippedKeys"/>).
        /// </summary>
        public bool OverwriteExisting { get; set; }

        /// <summary>
        /// Compute the result (migrated / skipped keys) without writing secrets.json. Default: false.
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// Only migrate keys for which this returns true. Null (default) migrates every key.
        /// Example: <c>k => k.EndsWith("Password") || k.EndsWith("Key")</c>
        /// </summary>
        public Func<string, bool> KeyFilter { get; set; }
    }

    /// <summary>
    /// Outcome of a <see cref="UserSecretsMigrator"/> run.
    /// </summary>
    public class MigrationResult
    {
        internal MigrationResult(string secretsFilePath, IReadOnlyList<string> migratedKeys, IReadOnlyList<string> skippedKeys)
        {
            SecretsFilePath = secretsFilePath;
            MigratedKeys = migratedKeys;
            SkippedKeys = skippedKeys;
        }

        /// <summary>Full path of the secrets.json that was written.</summary>
        public string SecretsFilePath { get; }

        /// <summary>Keys written to secrets.json by this run.</summary>
        public IReadOnlyList<string> MigratedKeys { get; }

        /// <summary>Keys that already existed in secrets.json and were left untouched.</summary>
        public IReadOnlyList<string> SkippedKeys { get; }
    }
}
