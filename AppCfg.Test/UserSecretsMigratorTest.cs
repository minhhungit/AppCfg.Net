using AppCfg.SettingStore;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Tests for migrating App.config/Web.config appSettings and connectionStrings into secrets.json")]
    public class UserSecretsMigratorTest
    {
        private const string TestUserSecretsId = "appcfg-test-migrator";
        private string _secretsPath;
        private string _workDir;

        [SetUp]
        public void Setup()
        {
            _secretsPath = UserSecretsStore.GetSecretsFilePath(TestUserSecretsId);
            DeleteSecretsDirectory();

            _workDir = Path.Combine(Path.GetTempPath(), "appcfg-migrator-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_workDir);

            UserSecretsStore.ClearCache();
            MyAppCfg.ResetConfiguration();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteSecretsDirectory();
            try { Directory.Delete(_workDir, true); } catch { /* ignore */ }
            UserSecretsStore.ClearCache();
            MyAppCfg.ResetConfiguration();
        }

        [Test]
        [Description("Copies every appSettings entry into secrets.json and reports the migrated keys")]
        public void MigrateFromConfigFile_CopiesAppSettingsToSecretsJson()
        {
            var configPath = WriteConfig(@"<configuration>
  <appSettings>
    <add key=""Database:Host"" value=""db.local"" />
    <add key=""Api:Key"" value=""sk_live_123"" />
  </appSettings>
</configuration>");

            var result = UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId);

            Assert.AreEqual(_secretsPath, result.SecretsFilePath, "Result should point at the secrets.json that was written");
            Assert.IsTrue(File.Exists(_secretsPath), "secrets.json should be created");
            CollectionAssert.AreEquivalent(new[] { "Database:Host", "Api:Key" }, result.MigratedKeys, "Both keys should be reported as migrated");

            var json = JObject.Parse(File.ReadAllText(_secretsPath));
            Assert.AreEqual("db.local", (string)json["Database:Host"], "Value should be written as a flat colon key");
            Assert.AreEqual("sk_live_123", (string)json["Api:Key"]);
        }

        [Test]
        [Description("connectionStrings entries are migrated under their name so [Option(Alias = name)] keeps working")]
        public void MigrateFromConfigFile_IncludesConnectionStringsByName()
        {
            var configPath = WriteConfig(@"<configuration>
  <connectionStrings>
    <add name=""MainDb"" connectionString=""Server=.;Database=Main;User Id=sa;Password=p@ss"" providerName=""System.Data.SqlClient"" />
  </connectionStrings>
</configuration>");

            var result = UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId);

            CollectionAssert.Contains(result.MigratedKeys, "MainDb");
            var json = JObject.Parse(File.ReadAllText(_secretsPath));
            Assert.AreEqual("Server=.;Database=Main;User Id=sa;Password=p@ss", (string)json["MainDb"]);
        }

        [Test]
        [Description("IncludeConnectionStrings = false leaves connectionStrings out")]
        public void MigrateFromConfigFile_IncludeConnectionStringsFalse_SkipsThem()
        {
            var configPath = WriteConfig(@"<configuration>
  <appSettings>
    <add key=""Plain"" value=""1"" />
  </appSettings>
  <connectionStrings>
    <add name=""MainDb"" connectionString=""Server=.;"" />
  </connectionStrings>
</configuration>");

            var result = UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId,
                new MigrationOptions { IncludeConnectionStrings = false });

            CollectionAssert.AreEquivalent(new[] { "Plain" }, result.MigratedKeys);
        }

        [Test]
        [Description("KeyFilter limits which keys are migrated")]
        public void MigrateFromConfigFile_KeyFilter_OnlyMigratesMatchingKeys()
        {
            var configPath = WriteConfig(@"<configuration>
  <appSettings>
    <add key=""Api:Key"" value=""secret"" />
    <add key=""Api:Timeout"" value=""30"" />
    <add key=""Smtp:Password"" value=""secret2"" />
  </appSettings>
</configuration>");

            var result = UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId,
                new MigrationOptions
                {
                    KeyFilter = key => key.EndsWith("Key", StringComparison.OrdinalIgnoreCase)
                              || key.EndsWith("Password", StringComparison.OrdinalIgnoreCase)
                });

            CollectionAssert.AreEquivalent(new[] { "Api:Key", "Smtp:Password" }, result.MigratedKeys);
            var json = JObject.Parse(File.ReadAllText(_secretsPath));
            Assert.IsNull(json["Api:Timeout"], "Filtered-out key must not be written");
        }

        [Test]
        [Description("A key that already exists in secrets.json is kept and reported as skipped unless OverwriteExisting is set")]
        public void Migrate_ExistingKeyInSecrets_IsKeptUnlessOverwrite()
        {
            WriteSecrets(@"{ ""Api:Key"": ""already-set-locally"" }");
            var configPath = WriteConfig(@"<configuration>
  <appSettings>
    <add key=""Api:Key"" value=""from-config"" />
    <add key=""Api:Other"" value=""other"" />
  </appSettings>
</configuration>");

            var kept = UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId);

            CollectionAssert.AreEquivalent(new[] { "Api:Other" }, kept.MigratedKeys, "Only the new key is migrated");
            CollectionAssert.AreEquivalent(new[] { "Api:Key" }, kept.SkippedKeys, "Existing key is reported as skipped");
            Assert.AreEqual("already-set-locally", (string)JObject.Parse(File.ReadAllText(_secretsPath))["Api:Key"]);

            var overwritten = UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId,
                new MigrationOptions { OverwriteExisting = true });

            CollectionAssert.Contains(overwritten.MigratedKeys, "Api:Key");
            Assert.AreEqual("from-config", (string)JObject.Parse(File.ReadAllText(_secretsPath))["Api:Key"]);
        }

        [Test]
        [Description("Secrets already in the file that are unrelated to the config are preserved, including nested ones")]
        public void Migrate_PreservesUnrelatedExistingSecrets()
        {
            WriteSecrets(@"{ ""Other"": { ""Nested"": ""keep-me"" } }");
            var configPath = WriteConfig(@"<configuration>
  <appSettings>
    <add key=""New:Key"" value=""new"" />
  </appSettings>
</configuration>");

            UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId);

            UserSecretsStore.ClearCache();
            MyAppCfg.Configure(envVarPrefix: "MIGTEST__", userSecretsId: TestUserSecretsId);
            var settings = MyAppCfg.Get<IMigratedSettings>();
            Assert.AreEqual("keep-me", settings.OtherNested, "Pre-existing nested secret must survive the merge");
            Assert.AreEqual("new", settings.NewKey);
        }

        [Test]
        [Description("Honours <appSettings file=\"...\"> - the external file adds and overrides keys")]
        public void MigrateFromConfigFile_HonoursExternalAppSettingsFile()
        {
            File.WriteAllText(Path.Combine(_workDir, "external.config"), @"<appSettings>
  <add key=""Shared"" value=""from-external"" />
  <add key=""OnlyExternal"" value=""ext"" />
</appSettings>");
            var configPath = WriteConfig(@"<configuration>
  <appSettings file=""external.config"">
    <add key=""Shared"" value=""from-main"" />
    <add key=""OnlyMain"" value=""main"" />
  </appSettings>
</configuration>");

            UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId);

            var json = JObject.Parse(File.ReadAllText(_secretsPath));
            Assert.AreEqual("from-external", (string)json["Shared"], "External file overrides the main file like .NET does");
            Assert.AreEqual("ext", (string)json["OnlyExternal"]);
            Assert.AreEqual("main", (string)json["OnlyMain"]);
        }

        [Test]
        [Description("Honours configSource=\"...\" on appSettings and connectionStrings")]
        public void MigrateFromConfigFile_HonoursConfigSource()
        {
            File.WriteAllText(Path.Combine(_workDir, "app.settings.config"), @"<appSettings>
  <add key=""FromSource"" value=""yes"" />
</appSettings>");
            File.WriteAllText(Path.Combine(_workDir, "conn.config"), @"<connectionStrings>
  <add name=""SrcDb"" connectionString=""Server=src;"" />
</connectionStrings>");
            var configPath = WriteConfig(@"<configuration>
  <appSettings configSource=""app.settings.config"" />
  <connectionStrings configSource=""conn.config"" />
</configuration>");

            var result = UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId);

            CollectionAssert.AreEquivalent(new[] { "FromSource", "SrcDb" }, result.MigratedKeys);
        }

        [Test]
        [Description("MigrateFromCurrentConfig reads the running application's App.config via ConfigurationManager")]
        public void MigrateFromCurrentConfig_ReadsRunningAppConfig()
        {
            var result = UserSecretsMigrator.MigrateFromCurrentConfig(TestUserSecretsId);

            var json = JObject.Parse(File.ReadAllText(_secretsPath));
            Assert.AreEqual("29", (string)json["Age"], "appSettings key from the test App.config should be migrated");
            Assert.That((string)json["myConn"], Does.Contain("Initial Catalog=Microsoft"), "connectionStrings entry should be migrated by name");
            CollectionAssert.Contains(result.MigratedKeys, "Age");
            Assert.IsNull(json["LocalSqlServer"], "Connection strings inherited from machine.config must not be migrated");
        }

        [Test]
        [Description("End-to-end: values migrated to secrets.json are picked up by Configure() ahead of App.config")]
        public void MigratedSecrets_AreReadableThroughConfigure()
        {
            var configPath = WriteConfig(@"<configuration>
  <appSettings>
    <add key=""New:Key"" value=""hello-from-secrets"" />
  </appSettings>
</configuration>");

            UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId);
            MyAppCfg.Configure(envVarPrefix: "MIGTEST__", userSecretsId: TestUserSecretsId);

            var settings = MyAppCfg.Get<IMigratedSettings>();
            Assert.AreEqual("hello-from-secrets", settings.NewKey);
        }

        [Test]
        [Description("A missing config file is reported as AppCfgException")]
        public void MigrateFromConfigFile_MissingFile_ThrowsAppCfgException()
        {
            var ex = Assert.Throws<AppCfgException>(() =>
                UserSecretsMigrator.MigrateFromConfigFile(Path.Combine(_workDir, "nope.config"), TestUserSecretsId));
            Assert.That(ex.Message, Does.Contain("nope.config"));
        }

        [Test]
        [Description("ReadConfigFile lets callers preview what would be migrated without writing anything")]
        public void ReadConfigFile_ReturnsSettingsWithoutWriting()
        {
            var configPath = WriteConfig(@"<configuration>
  <appSettings>
    <add key=""A"" value=""1"" />
    <remove key=""A"" />
    <add key=""B"" value=""2"" />
  </appSettings>
</configuration>");

            var settings = UserSecretsMigrator.ReadConfigFile(configPath);

            CollectionAssert.AreEquivalent(new[] { "B" }, settings.Keys.ToList(), "<remove> is honoured");
            Assert.IsFalse(File.Exists(_secretsPath), "Reading must not write secrets.json");
        }

        [Test]
        [Description("DryRun reports what would be migrated/skipped but writes nothing")]
        public void Migrate_DryRun_ReportsWithoutWriting()
        {
            WriteSecrets(@"{ ""Api:Key"": ""already-set-locally"" }");
            var configPath = WriteConfig(@"<configuration>
  <appSettings>
    <add key=""Api:Key"" value=""from-config"" />
    <add key=""Api:Other"" value=""other"" />
  </appSettings>
</configuration>");
            var before = File.ReadAllText(_secretsPath);

            var result = UserSecretsMigrator.MigrateFromConfigFile(configPath, TestUserSecretsId,
                new MigrationOptions { DryRun = true });

            CollectionAssert.AreEquivalent(new[] { "Api:Other" }, result.MigratedKeys);
            CollectionAssert.AreEquivalent(new[] { "Api:Key" }, result.SkippedKeys);
            Assert.AreEqual(_secretsPath, result.SecretsFilePath);
            Assert.AreEqual(before, File.ReadAllText(_secretsPath), "secrets.json must be untouched in a dry run");
        }

        [Test]
        [Description("A standalone external file whose root is <appSettings> (as used by file= / configSource=) is read directly")]
        public void MigrateFromConfigFile_StandaloneAppSettingsRoot_IsRead()
        {
            var path = Path.Combine(_workDir, "AppSettings.TEST.config");
            File.WriteAllText(path, "﻿" + @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<appSettings>
  <add key=""Logging"" value=""on"" />
  <add key=""Api:Key"" value=""k"" />
</appSettings>");

            var result = UserSecretsMigrator.MigrateFromConfigFile(path, TestUserSecretsId);

            CollectionAssert.AreEquivalent(new[] { "Logging", "Api:Key" }, result.MigratedKeys);
        }

        [Test]
        [Description("A standalone file whose root is <connectionStrings> is read directly")]
        public void MigrateFromConfigFile_StandaloneConnectionStringsRoot_IsRead()
        {
            var path = Path.Combine(_workDir, "connectionStrings.config");
            File.WriteAllText(path, "<connectionStrings><add name=\"MainDb\" connectionString=\"Server=.;\" /></connectionStrings>");

            var result = UserSecretsMigrator.MigrateFromConfigFile(path, TestUserSecretsId);

            CollectionAssert.AreEquivalent(new[] { "MainDb" }, result.MigratedKeys);
        }

        [Test]
        [Description("MigrateFromJsonFile flattens a .NET Core appsettings.json (nested objects, arrays) into secrets.json")]
        public void MigrateFromJsonFile_FlattensNestedJsonIntoSecrets()
        {
            var path = Path.Combine(_workDir, "appsettings.json");
            File.WriteAllText(path, @"{
  ""ConnectionStrings"": { ""MainDb"": ""Server=.;Password=p@ss"" },
  ""Api"": { ""Key"": ""sk_live"", ""Timeout"": 30 },
  ""Hosts"": [ ""a"", ""b"" ],
  ""Logging"": { ""LogLevel"": { ""Default"": ""Information"" } }
}");

            var result = UserSecretsMigrator.MigrateFromJsonFile(path, TestUserSecretsId,
                new MigrationOptions { KeyFilter = k => !k.StartsWith("Logging:") });

            CollectionAssert.AreEquivalent(
                new[] { "ConnectionStrings:MainDb", "Api:Key", "Api:Timeout", "Hosts:0", "Hosts:1" },
                result.MigratedKeys);
            var json = JObject.Parse(File.ReadAllText(_secretsPath));
            Assert.AreEqual("30", (string)json["Api:Timeout"], "Non-string JSON values are written as strings");
            Assert.AreEqual("Server=.;Password=p@ss", (string)json["ConnectionStrings:MainDb"]);
        }

        [Test]
        [Description("MigrateFromJsonFile reports a missing file as AppCfgException")]
        public void MigrateFromJsonFile_MissingFile_ThrowsAppCfgException()
        {
            var ex = Assert.Throws<AppCfgException>(() =>
                UserSecretsMigrator.MigrateFromJsonFile(Path.Combine(_workDir, "nope.json"), TestUserSecretsId));
            Assert.That(ex.Message, Does.Contain("nope.json"));
        }

        // ---- helpers

        private string WriteConfig(string xml)
        {
            var path = Path.Combine(_workDir, "App.config");
            File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + xml);
            return path;
        }

        private void WriteSecrets(string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_secretsPath));
            File.WriteAllText(_secretsPath, json);
        }

        private void DeleteSecretsDirectory()
        {
            var dir = Path.GetDirectoryName(_secretsPath);
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            catch { /* ignore */ }
        }

        public interface IMigratedSettings
        {
            [Option(Alias = "New:Key")]
            string NewKey { get; }

            [Option(Alias = "Other:Nested")]
            string OtherNested { get; }
        }
    }
}
