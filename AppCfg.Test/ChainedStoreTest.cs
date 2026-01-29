using AppCfg;
using AppCfg.SettingStore;
using NUnit.Framework;
using System;
using System.IO;

namespace AppCfg.Test
{
    [TestFixture]
    public class ChainedStoreTest
    {
        // For testing, we use a custom store identity
        // In your app, use: MyAppCfg.Configure() which uses MyAppCfg.DefaultStoreIdentity
        private const string TestStoreIdentity = "AppCfg:Test";
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
        public void ChainedStore_EnvironmentVariableFirst_ReturnsEnvVarValue()
        {
            // Arrange
            Environment.SetEnvironmentVariable("APPCFG__PriorityTest", "from-env");
            CreateTestSecretsFile(@"{ ""PriorityTest"": ""from-secrets"" }");

            // Register ChainedStore - it auto-registers UserSecretsStore internally!
            ChainedStore.Register(TestStoreIdentity, "APPCFG__", TestUserSecretsId);

            // Act
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - Env var has highest priority
            Assert.AreEqual("from-env", settings.PriorityTest);
        }

        [Test]
        public void ChainedStore_NoEnvVar_FallsBackToSecrets()
        {
            // Arrange - No env var, only secrets
            CreateTestSecretsFile(@"{ ""PriorityTest"": ""from-secrets"" }");

            // Register ChainedStore only - auto-handles everything!
            ChainedStore.Register(TestStoreIdentity, "APPCFG__", TestUserSecretsId);

            // Act
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - Falls back to secrets
            Assert.AreEqual("from-secrets", settings.PriorityTest);
        }

        [Test]
        public void ChainedStore_NoEnvVarOrSecrets_FallsBackToAppSettings()
        {
            // Arrange - No env var, no secrets, only AppSettings (Age=29 in App.config)
            ChainedStore.Register(TestStoreIdentity, "APPCFG__", TestUserSecretsId);

            // Act
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - Falls back to AppSettings
            Assert.AreEqual("29", settings.AppSettingValue);
        }

        [Test]
        public void ChainedStore_AllSourcesMissing_UsesDefaultValue()
        {
            // Arrange - Nothing set anywhere
            ChainedStore.Register(TestStoreIdentity, "APPCFG__", TestUserSecretsId);

            // Act
            var settings = MyAppCfg.Get<ITestChainedSettings>();

            // Assert - Uses default value
            Assert.AreEqual("default-value", settings.PriorityTest);
        }

        private void CreateTestSecretsFile(string content)
        {
            Directory.CreateDirectory(_testSecretsDirectory);
            File.WriteAllText(_testSecretsPath, content);
        }

        // Test interface
        [DefaultOption(StoreType = SettingStoreType.Custom,
                       StoreIdentity = TestStoreIdentity)]
        public interface ITestChainedSettings
        {
            [Option(Alias = "PriorityTest", DefaultValue = "default-value")]
            string PriorityTest { get; }

            [Option(Alias = "Age", DefaultValue = "")]
            string AppSettingValue { get; }
        }
    }
}
