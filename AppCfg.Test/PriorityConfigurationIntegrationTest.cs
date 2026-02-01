using AppCfg.SettingStore;
using NUnit.Framework;
using System;
using System.IO;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Integration tests for priority-based configuration loading (Environment Variables > User Secrets > App.config)")]
    public class PriorityConfigurationIntegrationTest
    {
        private const string TestUserSecretsId = "appcfg-integration-test";
        private string _testSecretsPath;
        private string _testSecretsDirectory;

        [SetUp]
        public void Setup()
        {
            // Determine the secrets path
            string basePath;
            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                basePath = Path.Combine(home, ".microsoft", "usersecrets");
            }
            else
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                basePath = Path.Combine(appData, "Microsoft", "UserSecrets");
            }

            _testSecretsDirectory = Path.Combine(basePath, TestUserSecretsId);
            _testSecretsPath = Path.Combine(_testSecretsDirectory, "secrets.json");

            // Clean up
            if (Directory.Exists(_testSecretsDirectory))
            {
                Directory.Delete(_testSecretsDirectory, true);
            }

            // Clear environment variables
            Environment.SetEnvironmentVariable("INTTEST__TestKey", null);
            Environment.SetEnvironmentVariable("INTTEST__Priority1", null);
            Environment.SetEnvironmentVariable("INTTEST__Priority2", null);
            Environment.SetEnvironmentVariable("INTTEST__Priority3", null);

            // Clear UserSecretsStore cache
            UserSecretsStore.ClearCache();

            // Note: Test values are configured in App.config
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testSecretsDirectory))
            {
                Directory.Delete(_testSecretsDirectory, true);
            }

            Environment.SetEnvironmentVariable("INTTEST__TestKey", null);
            Environment.SetEnvironmentVariable("INTTEST__Priority1", null);
            Environment.SetEnvironmentVariable("INTTEST__Priority2", null);
            Environment.SetEnvironmentVariable("INTTEST__Priority3", null);

            UserSecretsStore.ClearCache();
        }

        [Test]
        [Description("Verifies that environment variable takes highest priority when all sources have a value")]
        public void Configure_WithAllSources_EnvironmentVariableWins()
        {
            // Arrange
            CreateTestSecretsFile(@"{ ""Priority1"": ""from-secrets"" }");
            Environment.SetEnvironmentVariable("INTTEST__Priority1", "from-env");

            MyAppCfg.Configure(
                envVarPrefix: "INTTEST__",
                userSecretsId: TestUserSecretsId
            );

            // Act
            var settings = MyAppCfg.Get<IPriorityTestSettings>();

            // Assert - Environment variable has highest priority
            Assert.AreEqual("from-env", settings.Priority1, "Environment variable should take precedence over user secrets");
        }

        [Test]
        [Description("Verifies that user secrets takes precedence over App.config when no environment variable exists")]
        public void Configure_NoEnvVar_UserSecretsWins()
        {
            // Arrange
            CreateTestSecretsFile(@"{ ""Priority2"": ""from-secrets"" }");

            MyAppCfg.Configure(
                envVarPrefix: "INTTEST__",
                userSecretsId: TestUserSecretsId
            );

            // Act
            var settings = MyAppCfg.Get<IPriorityTestSettings>();

            // Assert - User secrets beat App.config
            Assert.AreEqual("from-secrets", settings.Priority2, "User secrets should take precedence over App.config");
        }

        [Test]
        [Description("Verifies that App.config is used as fallback when no environment variable or user secret exists")]
        public void Configure_NoEnvVarOrSecrets_AppConfigWins()
        {
            // Arrange
            MyAppCfg.Configure(
                envVarPrefix: "INTTEST__",
                userSecretsId: TestUserSecretsId
            );

            // Act
            var settings = MyAppCfg.Get<IPriorityTestSettings>();

            // Assert - Falls back to App.config
            Assert.AreEqual("appconfig-priority3", settings.Priority3, "App.config should be used when no env var or secrets exist");
        }

        [Test]
        [Description("Verifies that user secrets are skipped when userSecretsId is null")]
        public void Configure_WithoutUserSecretsId_SkipsUserSecrets()
        {
            // Arrange
            CreateTestSecretsFile(@"{ ""Priority2"": ""from-secrets"" }");

            MyAppCfg.Configure(
                envVarPrefix: "INTTEST__",
                userSecretsId: null // Explicitly skip user secrets
            );

            // Act
            var settings = MyAppCfg.Get<IPriorityTestSettings>();

            // Assert - Should skip secrets and use App.config
            Assert.AreEqual("appconfig-priority2", settings.Priority2, "With null userSecretsId, should skip secrets and use App.config");
        }

        [Test]
        [Description("Verifies that environment variables are skipped when envVarPrefix is null")]
        public void Configure_WithoutEnvVarPrefix_SkipsEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("INTTEST__Priority1", "from-env");
            CreateTestSecretsFile(@"{ ""Priority1"": ""from-secrets"" }");

            MyAppCfg.Configure(
                envVarPrefix: null, // Explicitly skip env vars
                userSecretsId: TestUserSecretsId
            );

            // Act
            var settings = MyAppCfg.Get<IPriorityTestSettings>();

            // Assert - Should skip env vars and use secrets
            Assert.AreEqual("from-secrets", settings.Priority1, "With null envVarPrefix, should skip env vars and use secrets");
        }

        [Test]
        [Description("Verifies that calling Configure multiple times uses the last configuration")]
        public void Configure_CalledMultipleTimes_LastCallWins()
        {
            // Arrange
            MyAppCfg.Configure(envVarPrefix: "FIRST__", userSecretsId: null);
            MyAppCfg.Configure(envVarPrefix: "INTTEST__", userSecretsId: TestUserSecretsId);

            CreateTestSecretsFile(@"{ ""Priority2"": ""from-secrets"" }");

            // Act
            var settings = MyAppCfg.Get<IPriorityTestSettings>();

            // Assert - Should use the second Configure() call
            Assert.AreEqual("from-secrets", settings.Priority2, "Last Configure() call should take effect");
        }

        [Test]
        [Description("Verifies that without calling Configure(), only App.config is used (backward compatibility)")]
        public void WithoutConfigure_UsesAppConfigOnly()
        {
            // Arrange - Don't call Configure()

            // Act
            var settings = MyAppCfg.Get<IPriorityTestSettings>();

            // Assert - Should only use App.config (traditional behavior)
            Assert.AreEqual("appconfig-priority1", settings.Priority1, "Without Configure(), Priority1 should use App.config");
            Assert.AreEqual("appconfig-priority2", settings.Priority2, "Without Configure(), Priority2 should use App.config");
            Assert.AreEqual("appconfig-priority3", settings.Priority3, "Without Configure(), Priority3 should use App.config");
        }

        private void CreateTestSecretsFile(string content)
        {
            Directory.CreateDirectory(_testSecretsDirectory);
            File.WriteAllText(_testSecretsPath, content);
        }

        // Test interface
        public interface IPriorityTestSettings
        {
            [Option(Alias = "Priority1")]
            string Priority1 { get; }

            [Option(Alias = "Priority2")]
            string Priority2 { get; }

            [Option(Alias = "Priority3")]
            string Priority3 { get; }
        }
    }
}
