using AppCfg;
using AppCfg.SettingStore;
using NUnit.Framework;
using System;
using System.IO;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Tests for ChainedStore functionality with priority-based configuration loading")]
    public class ChainedStoreTest
    {
        // For testing, we use a custom profile key
        // In your app, use: MyAppCfg.Configure() which uses MyAppCfg.DefaultProfileKey
        private const string TestUserSecretsId = "appcfg-test-chained";
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
            Environment.SetEnvironmentVariable("APPCFG__TestKey", null);
            Environment.SetEnvironmentVariable("APPCFG__PriorityTest", null);

            // Clear UserSecretsStore cache to ensure fresh reads
            UserSecretsStore.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testSecretsDirectory))
            {
                Directory.Delete(_testSecretsDirectory, true);
            }

            Environment.SetEnvironmentVariable("APPCFG__TestKey", null);
            Environment.SetEnvironmentVariable("APPCFG__PriorityTest", null);

            // Clear UserSecretsStore cache after test
            UserSecretsStore.ClearCache();
        }

        [Test]
        [Description("Verifies that environment variable takes highest priority in the chained store")]
        public void ChainedStore_EnvironmentVariableFirst_ReturnsEnvVarValue()
        {
            // Arrange
            Environment.SetEnvironmentVariable("APPCFG__PriorityTest", "from-env");
            CreateTestSecretsFile(@"{ ""PriorityTest"": ""from-secrets"" }");

            MyAppCfg.Configure(
                envVarPrefix: "APPCFG__",
                userSecretsId: TestUserSecretsId
            );

            // Act
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - Env var has highest priority
            Assert.AreEqual("from-env", settings.PriorityTest, "Environment variable should take highest priority in chained store");
        }

        [Test]
        [Description("Verifies that user secrets is used when no environment variable exists")]
        public void ChainedStore_NoEnvVar_FallsBackToSecrets()
        {
            // Arrange
            CreateTestSecretsFile(@"{ ""PriorityTest"": ""from-secrets"" }");

            // Register ChainedStore only - auto-handles everything!
            MyAppCfg.Configure(
                envVarPrefix: "APPCFG__",
                userSecretsId: TestUserSecretsId
            );

            // Act
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - Falls back to secrets
            Assert.AreEqual("from-secrets", settings.PriorityTest, "Chained store should fall back to user secrets when env var is missing");
        }

        [Test]
        [Description("Verifies that App.config is used when no environment variable or user secret exists")]
        public void ChainedStore_NoEnvVarOrSecrets_FallsBackToAppSettings()
        {
            // Arrange
            MyAppCfg.Configure(
                envVarPrefix: "APPCFG__",
                userSecretsId: TestUserSecretsId
            );

            // Act
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - Falls back to AppSettings
            Assert.AreEqual("29", settings.AppSettingValue, "Chained store should fall back to App.config when env var and secrets are missing");
        }

        [Test]
        [Description("Verifies that DefaultValue is used when all configuration sources are missing")]
        public void ChainedStore_AllSourcesMissing_UsesDefaultValue()
        {
            // Arrange
            MyAppCfg.Configure(
                 envVarPrefix: "APPCFG__",
                 userSecretsId: TestUserSecretsId
             );

            // Act
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - Uses default value
            Assert.AreEqual("default-value", settings.PriorityTest, "Chained store should use DefaultValue when all sources are missing");
        }

        [Test]
        [Description("Verifies that a malformed secrets.json is reported instead of silently ignored when using Configure()")]
        public void ChainedStore_InvalidSecretsJson_ThrowsAppCfgException()
        {
            // Arrange
            CreateTestSecretsFile("{ invalid json }");

            MyAppCfg.Configure(
                envVarPrefix: "APPCFG__",
                userSecretsId: TestUserSecretsId
            );

            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() => MyAppCfg.Get<ITestChainedSettings>());
            Assert.That(ex.Message, Does.Contain("secrets"), "Error should point at the secrets file");
        }

        [Test]
        [Description("Verifies that ResetConfiguration() turns priority-based loading off again")]
        public void ChainedStore_AfterResetConfiguration_UsesAppConfigOnly()
        {
            // Arrange
            CreateTestSecretsFile(@"{ ""PriorityTest"": ""from-secrets"" }");
            MyAppCfg.Configure(
                envVarPrefix: "APPCFG__",
                userSecretsId: TestUserSecretsId
            );
            Assert.AreEqual("from-secrets", MyAppCfg.Get<ITestChainedSettings>().PriorityTest, "Precondition: secrets are read while configured");

            // Act
            MyAppCfg.ResetConfiguration();
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - secrets are no longer consulted, App.config has no value, so DefaultValue applies
            Assert.AreEqual("default-value", settings.PriorityTest, "After ResetConfiguration() only App.config should be used");
        }

        private void CreateTestSecretsFile(string content)
        {
            Directory.CreateDirectory(_testSecretsDirectory);
            File.WriteAllText(_testSecretsPath, content);
        }

        // Test interface - when Configure() is called, automatically uses priority-based loading
        public interface ITestChainedSettings
        {
            [Option(Alias = "PriorityTest", DefaultValue = "default-value")]
            string PriorityTest { get; }

            [Option(Alias = "Age")]
            string AppSettingValue { get; }
        }
    }
}
