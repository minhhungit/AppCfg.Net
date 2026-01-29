using AppCfg;
using AppCfg.SettingStore;
using NUnit.Framework;
using System;
using System.IO;

namespace AppCfg.Test
{
    [TestFixture]
    public class UserSecretsStoreTest
    {
        private const string TestUserSecretsId = "appcfg-test-usersecrets";
        private const string TestStoreIdentity = "UserSecrets:appcfg-test-usersecrets";
        private string _testSecretsPath;
        private string _testSecretsDirectory;

        [SetUp]
        public void Setup()
        {
            // Determine the secrets path based on platform
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

            // Clear the cache and register the store before each test
            UserSecretsStore.ClearCache();
            UserSecretsStore.Register(TestUserSecretsId, TestStoreIdentity);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test secrets directory
            try
            {
                if (Directory.Exists(_testSecretsDirectory))
                {
                    Directory.Delete(_testSecretsDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }

            // Clear cache after test
            UserSecretsStore.ClearCache();
        }

        [Test]
        public void LoadSecrets_WithValidJson_ReturnsCorrectValues()
        {
            // Arrange
            CreateTestSecretsFile(@"{
                ""TestKey"": ""TestValue"",
                ""ApiKey"": ""sk_test_12345""
            }");

            // Act
            var settings = MyAppCfg.Get<ITestSecretSettings>();

            // Assert
            Assert.AreEqual("TestValue", settings.TestKey);
            Assert.AreEqual("sk_test_12345", settings.ApiKey);
        }

        [Test]
        public void LoadSecrets_WithHierarchicalKeys_ReturnsCorrectValues()
        {
            // Arrange
            CreateTestSecretsFile(@"{
                ""Database:Host"": ""localhost"",
                ""Database:Port"": ""5432"",
                ""Database:Password"": ""secret123""
            }");

            // Act
            var settings = MyAppCfg.Get<ITestHierarchicalSettings>();

            // Assert
            Assert.AreEqual("localhost", settings.DatabaseHost);
            Assert.AreEqual(5432, settings.DatabasePort);
            Assert.AreEqual("secret123", settings.DatabasePassword);
        }

        [Test]
        public void LoadSecrets_WithNestedJsonObject_FlattenCorrectly()
        {
            // Arrange
            CreateTestSecretsFile(@"{
                ""Database"": {
                    ""Host"": ""localhost"",
                    ""Port"": ""5432""
                }
            }");

            // Act
            var settings = MyAppCfg.Get<ITestHierarchicalSettings>();

            // Assert
            Assert.AreEqual("localhost", settings.DatabaseHost);
            Assert.AreEqual(5432, settings.DatabasePort);
        }

        [Test]
        public void LoadSecrets_WithMissingFile_UsesDefaultValues()
        {
            // Arrange - Don't create secrets file
            // Act
            var settings = MyAppCfg.Get<ITestSecretSettings>();

            // Assert - Should use default values
            Assert.AreEqual("default-value", settings.TestKey);
            Assert.AreEqual("default-api-key", settings.ApiKey);
        }

        [Test]
        public void LoadSecrets_WithMissingKeys_UsesDefaultValues()
        {
            // Arrange - Create file with only one key
            CreateTestSecretsFile(@"{
                ""TestKey"": ""TestValue""
            }");

            // Act
            var settings = MyAppCfg.Get<ITestSecretSettings>();

            // Assert
            Assert.AreEqual("TestValue", settings.TestKey);
            Assert.AreEqual("default-api-key", settings.ApiKey); // Should use default
        }

        [Test]
        public void LoadSecrets_WithInvalidJson_ThrowsException()
        {
            // Arrange
            CreateTestSecretsFile("{ invalid json }");

            // Act & Assert
            Assert.Throws<AppCfgException>(() => MyAppCfg.Get<ITestSecretSettings>());
        }

        [Test]
        public void LoadSecrets_WithEmptyFile_UsesDefaultValues()
        {
            // Arrange
            CreateTestSecretsFile("");

            // Act
            var settings = MyAppCfg.Get<ITestSecretSettings>();

            // Assert - Should use default values
            Assert.AreEqual("default-value", settings.TestKey);
            Assert.AreEqual("default-api-key", settings.ApiKey);
        }

        [Test]
        public void LoadSecrets_CachesResults()
        {
            // Arrange
            CreateTestSecretsFile(@"{
                ""TestKey"": ""FirstValue""
            }");

            // Act - Load first time
            var settings1 = MyAppCfg.Get<ITestSecretSettings>();
            Assert.AreEqual("FirstValue", settings1.TestKey);

            // Change the file
            CreateTestSecretsFile(@"{
                ""TestKey"": ""SecondValue""
            }");

            // Load again - should still get cached value
            var settings2 = MyAppCfg.Get<ITestSecretSettings>();
            Assert.AreEqual("FirstValue", settings2.TestKey); // Still cached

            // Clear cache and load again
            UserSecretsStore.ClearCache(TestUserSecretsId);
            var settings3 = MyAppCfg.Get<ITestSecretSettings>();
            Assert.AreEqual("SecondValue", settings3.TestKey); // New value
        }

        [Test]
        public void Register_WithNullUserSecretsId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => UserSecretsStore.Register(null));
        }

        [Test]
        public void Register_WithEmptyUserSecretsId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => UserSecretsStore.Register(""));
        }

        [Test]
        public void LoadSecrets_WithTypeParsing_ParsesCorrectly()
        {
            // Arrange
            CreateTestSecretsFile(@"{
                ""IntValue"": ""42"",
                ""BoolValue"": ""true"",
                ""GuidValue"": ""12345678-1234-1234-1234-123456789abc""
            }");

            // Act
            var settings = MyAppCfg.Get<ITestTypedSettings>();

            // Assert
            Assert.AreEqual(42, settings.IntValue);
            Assert.AreEqual(true, settings.BoolValue);
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-123456789abc"), settings.GuidValue);
        }

        private void CreateTestSecretsFile(string content)
        {
            Directory.CreateDirectory(_testSecretsDirectory);
            File.WriteAllText(_testSecretsPath, content);
        }

        // Test interfaces - using DefaultOption to avoid repetition
        [DefaultOption(StoreType = SettingStoreType.Custom,
                       StoreIdentity = TestStoreIdentity)]
        public interface ITestSecretSettings
        {
            [Option(Alias = "TestKey", DefaultValue = "default-value")]
            string TestKey { get; }

            [Option(Alias = "ApiKey", DefaultValue = "default-api-key")]
            string ApiKey { get; }
        }

        [DefaultOption(StoreType = SettingStoreType.Custom,
                       StoreIdentity = TestStoreIdentity)]
        public interface ITestHierarchicalSettings
        {
            [Option(Alias = "Database:Host", DefaultValue = "")]
            string DatabaseHost { get; }

            [Option(Alias = "Database:Port", DefaultValue = 0)]
            int DatabasePort { get; }

            [Option(Alias = "Database:Password", DefaultValue = "")]
            string DatabasePassword { get; }
        }

        [DefaultOption(StoreType = SettingStoreType.Custom,
                       StoreIdentity = TestStoreIdentity)]
        public interface ITestTypedSettings
        {
            [Option(Alias = "IntValue", DefaultValue = 0)]
            int IntValue { get; }

            [Option(Alias = "BoolValue", DefaultValue = false)]
            bool BoolValue { get; }

            [Option(Alias = "GuidValue")]
            Guid GuidValue { get; }
        }
    }
}
