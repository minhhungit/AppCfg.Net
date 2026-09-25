using AppCfg;
using AppCfg.SettingStore;
using NUnit.Framework;
using System;
using System.IO;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Tests for loading configuration from user secrets JSON file")]
    public class UserSecretsStoreTest
    {
        private const string TestUserSecretsId = "appcfg-test-usersecrets";
        private const string TestProfileKey = "UserSecrets:appcfg-test-usersecrets";
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
            UserSecretsStore.Register(TestUserSecretsId, TestProfileKey);
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
        [Description("Verifies that valid JSON secrets file is parsed and values are returned correctly")]
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
            Assert.AreEqual("TestValue", settings.TestKey, "TestKey should be loaded from secrets.json");
            Assert.AreEqual("sk_test_12345", settings.ApiKey, "ApiKey should be loaded from secrets.json");
        }

        [Test]
        [Description("Verifies that hierarchical keys with colon separator are correctly loaded")]
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
            Assert.AreEqual("localhost", settings.DatabaseHost, "Database:Host should be loaded correctly");
            Assert.AreEqual(5432, settings.DatabasePort, "Database:Port should be parsed as integer");
            Assert.AreEqual("secret123", settings.DatabasePassword, "Database:Password should be loaded correctly");
        }

        [Test]
        [Description("Verifies that nested JSON objects are flattened to hierarchical keys")]
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
            Assert.AreEqual("localhost", settings.DatabaseHost, "Nested Database.Host should be flattened to Database:Host");
            Assert.AreEqual(5432, settings.DatabasePort, "Nested Database.Port should be flattened and parsed as integer");
        }

        [Test]
        [Description("Verifies that missing secrets file falls back to DefaultValue attribute")]
        public void LoadSecrets_WithMissingFile_UsesDefaultValues()
        {
            // Arrange - Don't create secrets file
            // Act
            var settings = MyAppCfg.Get<ITestSecretSettings>();

            // Assert - Should use default values
            Assert.AreEqual("default-value", settings.TestKey, "Missing file should use DefaultValue for TestKey");
            Assert.AreEqual("default-api-key", settings.ApiKey, "Missing file should use DefaultValue for ApiKey");
        }

        [Test]
        [Description("Verifies that missing keys in secrets file fall back to DefaultValue attribute")]
        public void LoadSecrets_WithMissingKeys_UsesDefaultValues()
        {
            // Arrange - Create file with only one key
            CreateTestSecretsFile(@"{
                ""TestKey"": ""TestValue""
            }");

            // Act
            var settings = MyAppCfg.Get<ITestSecretSettings>();

            // Assert
            Assert.AreEqual("TestValue", settings.TestKey, "Existing key should return its value");
            Assert.AreEqual("default-api-key", settings.ApiKey, "Missing key should use DefaultValue");
        }

        [Test]
        [Description("Verifies that invalid JSON format throws AppCfgException")]
        public void LoadSecrets_WithInvalidJson_ThrowsException()
        {
            // Arrange
            CreateTestSecretsFile("{ invalid json }");

            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() => MyAppCfg.Get<ITestSecretSettings>());
            Assert.IsNotNull(ex, "AppCfgException should be thrown for invalid JSON");
        }

        [Test]
        [Description("Verifies that empty secrets file falls back to DefaultValue attribute")]
        public void LoadSecrets_WithEmptyFile_UsesDefaultValues()
        {
            // Arrange
            CreateTestSecretsFile("");

            // Act
            var settings = MyAppCfg.Get<ITestSecretSettings>();

            // Assert - Should use default values
            Assert.AreEqual("default-value", settings.TestKey, "Empty file should use DefaultValue for TestKey");
            Assert.AreEqual("default-api-key", settings.ApiKey, "Empty file should use DefaultValue for ApiKey");
        }

        [Test]
        [Description("Verifies that secrets are cached and subsequent loads use cached values")]
        public void LoadSecrets_CachesResults()
        {
            // Arrange
            CreateTestSecretsFile(@"{
                ""TestKey"": ""FirstValue""
            }");

            // Act - Load first time
            var settings1 = MyAppCfg.Get<ITestSecretSettings>();
            Assert.AreEqual("FirstValue", settings1.TestKey, "First load should return 'FirstValue'");

            // Change the file
            CreateTestSecretsFile(@"{
                ""TestKey"": ""SecondValue""
            }");

            // Load again - should still get cached value
            var settings2 = MyAppCfg.Get<ITestSecretSettings>();
            Assert.AreEqual("FirstValue", settings2.TestKey, "Second load should return cached 'FirstValue'");

            // Clear cache and load again
            UserSecretsStore.ClearCache(TestUserSecretsId);
            var settings3 = MyAppCfg.Get<ITestSecretSettings>();
            Assert.AreEqual("SecondValue", settings3.TestKey, "After cache clear, should return new 'SecondValue'");
        }

        [Test]
        [Description("Verifies that Register throws ArgumentException when userSecretsId is null")]
        public void Register_WithNullUserSecretsId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => UserSecretsStore.Register(null));
            Assert.IsNotNull(ex, "ArgumentException should be thrown for null userSecretsId");
        }

        [Test]
        [Description("Verifies that Register throws ArgumentException when userSecretsId is empty string")]
        public void Register_WithEmptyUserSecretsId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => UserSecretsStore.Register(""));
            Assert.IsNotNull(ex, "ArgumentException should be thrown for empty userSecretsId");
        }

        [Test]
        [Description("Verifies that secrets values are correctly parsed to different types")]
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
            Assert.AreEqual(42, settings.IntValue, "Integer value should be parsed correctly from secrets");
            Assert.AreEqual(true, settings.BoolValue, "Boolean value should be parsed correctly from secrets");
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-123456789abc"), settings.GuidValue, "Guid value should be parsed correctly from secrets");
        }

        [Test]
        [Description("Verifies that JSON arrays are flattened to indexed keys (Hosts:0, Hosts:1) like .NET Core")]
        public void LoadSecrets_WithArrayValues_FlattensWithIndexKeys()
        {
            // Arrange
            CreateTestSecretsFile(@"{
                ""Hosts"": [ ""alpha"", ""beta"" ]
            }");

            // Act
            var settings = MyAppCfg.Get<ITestArraySettings>();

            // Assert
            Assert.AreEqual("alpha", settings.FirstHost, "Hosts[0] should be exposed as Hosts:0");
            Assert.AreEqual("beta", settings.SecondHost, "Hosts[1] should be exposed as Hosts:1");
        }

        [Test]
        [Description("Verifies that a JSON null value is treated as missing so DefaultValue applies")]
        public void LoadSecrets_WithNullValue_UsesDefaultValue()
        {
            // Arrange
            CreateTestSecretsFile(@"{
                ""TestKey"": null,
                ""ApiKey"": ""present""
            }");

            // Act
            var settings = MyAppCfg.Get<ITestSecretSettings>();

            // Assert
            Assert.AreEqual("default-value", settings.TestKey, "JSON null should fall back to DefaultValue");
            Assert.AreEqual("present", settings.ApiKey, "Other keys should still load");
        }

        [Test]
        [Description("Verifies that GetSecretsFilePath returns the platform-specific secrets.json location")]
        public void GetSecretsFilePath_ReturnsPlatformSpecificPath()
        {
            // Act
            var path = UserSecretsStore.GetSecretsFilePath(TestUserSecretsId);

            // Assert
            Assert.AreEqual(_testSecretsPath, path, "Path should match the .NET Core Secret Manager convention");
        }

        [Test]
        [Description("Verifies that the root environment variable moves the secrets file out of the user profile")]
        public void GetSecretsFilePath_WithRootEnvironmentVariable_UsesRoot()
        {
            var root = Path.Combine(Path.GetTempPath(), "appcfg-test-secrets-root");
            try
            {
                // Arrange
                Environment.SetEnvironmentVariable(UserSecretsStore.RootEnvironmentVariable, root);

                // Act
                var path = UserSecretsStore.GetSecretsFilePath(TestUserSecretsId);

                // Assert
                Assert.AreEqual(Path.Combine(root, TestUserSecretsId, "secrets.json"), path);
            }
            finally
            {
                Environment.SetEnvironmentVariable(UserSecretsStore.RootEnvironmentVariable, null);
            }
        }

        [Test]
        [Description("Verifies that secrets are loaded from the root environment variable folder when it is set")]
        public void LoadSecrets_WithRootEnvironmentVariable_ReadsFromRoot()
        {
            var root = Path.Combine(Path.GetTempPath(), "appcfg-test-secrets-root");
            try
            {
                // Arrange: same key in the profile file and the root file, the root file must win
                CreateTestSecretsFile(@"{ ""TestKey"": ""ProfileValue"" }");
                var rootDirectory = Path.Combine(root, TestUserSecretsId);
                Directory.CreateDirectory(rootDirectory);
                File.WriteAllText(Path.Combine(rootDirectory, "secrets.json"), @"{ ""TestKey"": ""RootValue"" }");
                Environment.SetEnvironmentVariable(UserSecretsStore.RootEnvironmentVariable, root);

                // Act
                var settings = MyAppCfg.Get<ITestSecretSettings>();

                // Assert
                Assert.AreEqual("RootValue", settings.TestKey, "TestKey should be loaded from the root folder");
                Assert.AreEqual("default-api-key", settings.ApiKey, "Missing key should use the default value");
            }
            finally
            {
                Environment.SetEnvironmentVariable(UserSecretsStore.RootEnvironmentVariable, null);
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        private void CreateTestSecretsFile(string content)
        {
            Directory.CreateDirectory(_testSecretsDirectory);
            File.WriteAllText(_testSecretsPath, content);
        }

        // Test interfaces - using DefaultOption to avoid repetition
        [DefaultOption(ProfileKey = TestProfileKey)]
        public interface ITestSecretSettings
        {
            [Option(Alias = "TestKey", DefaultValue = "default-value")]
            string TestKey { get; }

            [Option(Alias = "ApiKey", DefaultValue = "default-api-key")]
            string ApiKey { get; }
        }

        [DefaultOption(ProfileKey = TestProfileKey)]
        public interface ITestHierarchicalSettings
        {
            [Option(Alias = "Database:Host")]
            string DatabaseHost { get; }

            [Option(Alias = "Database:Port")]
            int DatabasePort { get; }

            [Option(Alias = "Database:Password")]
            string DatabasePassword { get; }
        }

        [DefaultOption(ProfileKey = TestProfileKey)]
        public interface ITestArraySettings
        {
            [Option(Alias = "Hosts:0")]
            string FirstHost { get; }

            [Option(Alias = "Hosts:1")]
            string SecondHost { get; }
        }

        [DefaultOption(ProfileKey = TestProfileKey)]
        public interface ITestTypedSettings
        {
            [Option(Alias = "IntValue")]
            int IntValue { get; }

            [Option(Alias = "BoolValue")]
            bool BoolValue { get; }

            [Option(Alias = "GuidValue")]
            Guid GuidValue { get; }
        }
    }
}
