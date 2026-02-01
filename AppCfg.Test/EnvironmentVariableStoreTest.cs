using AppCfg;
using AppCfg.SettingStore;
using NUnit.Framework;
using System;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Tests for loading configuration from environment variables")]
    public class EnvironmentVariableStoreTest
    {
        private const string TestPrefix = "APPCFGTEST__";
        private const string TestProfileKey = "EnvironmentVariables:" + TestPrefix;

        [TearDown]
        public void TearDown()
        {
            // Clean up environment variables after each test
            Environment.SetEnvironmentVariable($"{TestPrefix}TestKey", null);
            Environment.SetEnvironmentVariable($"{TestPrefix}ApiKey", null);
            Environment.SetEnvironmentVariable($"{TestPrefix}Database__Host", null);
            Environment.SetEnvironmentVariable($"{TestPrefix}Database__Port", null);
            Environment.SetEnvironmentVariable($"{TestPrefix}IntValue", null);
            Environment.SetEnvironmentVariable($"{TestPrefix}BoolValue", null);
            Environment.SetEnvironmentVariable($"{TestPrefix}GuidValue", null);
        }

        [Test]
        [Description("Verifies that a simple key-value pair is correctly loaded from environment variable")]
        public void LoadFromEnvironmentVariable_SimpleKey_ReturnsCorrectValue()
        {
            // Arrange
            Environment.SetEnvironmentVariable($"{TestPrefix}TestKey", "test-value");
            EnvironmentVariableStore.Register(TestPrefix, TestProfileKey);

            // Act
            var settings = MyAppCfg.Get<ITestEnvSettings>();

            // Assert
            Assert.AreEqual("test-value", settings.TestKey, "TestKey should be loaded from environment variable");
        }

        [Test]
        [Description("Verifies that hierarchical keys with double underscore separator are correctly loaded")]
        public void LoadFromEnvironmentVariable_HierarchicalKey_ReturnsCorrectValue()
        {
            // Arrange
            Environment.SetEnvironmentVariable($"{TestPrefix}Database__Host", "localhost");
            Environment.SetEnvironmentVariable($"{TestPrefix}Database__Port", "5432");
            EnvironmentVariableStore.Register(TestPrefix, TestProfileKey);

            // Act
            var settings = MyAppCfg.Get<ITestHierarchicalEnvSettings>();

            // Assert
            Assert.AreEqual("localhost", settings.DatabaseHost, "Database:Host should be loaded from hierarchical env var");
            Assert.AreEqual(5432, settings.DatabasePort, "Database:Port should be loaded and parsed as integer");
        }

        [Test]
        [Description("Verifies that missing environment variable falls back to DefaultValue attribute")]
        public void LoadFromEnvironmentVariable_MissingVariable_UsesDefaultValue()
        {
            // Arrange - Don't set environment variable
            EnvironmentVariableStore.Register(TestPrefix, TestProfileKey);

            // Act
            var settings = MyAppCfg.Get<ITestEnvSettings>();

            // Assert - Should use default value
            Assert.AreEqual("default-value", settings.TestKey, "Missing env var should fall back to DefaultValue");
        }

        [Test]
        [Description("Verifies that setting environment variable to empty string uses DefaultValue (as empty deletes the var)")]
        public void LoadFromEnvironmentVariable_EmptyValue_UsesDefaultValue()
        {
            // Arrange
            // Note: Setting env var to empty string ("") actually DELETES it in .NET
            // So this tests that deleted/missing variables use default values
            Environment.SetEnvironmentVariable($"{TestPrefix}TestKey", "");
            EnvironmentVariableStore.Register(TestPrefix, TestProfileKey);

            // Act
            var settings = MyAppCfg.Get<ITestEnvSettings>();

            // Assert - Setting to "" deletes the var, so default value is used
            Assert.AreEqual("default-value", settings.TestKey, "Empty env var should be treated as deleted and use DefaultValue");
        }

        [Test]
        [Description("Verifies that environment variable values are correctly parsed to different types")]
        public void LoadFromEnvironmentVariable_WithTypeParsing_ParsesCorrectly()
        {
            // Arrange
            Environment.SetEnvironmentVariable($"{TestPrefix}IntValue", "42");
            Environment.SetEnvironmentVariable($"{TestPrefix}BoolValue", "true");
            Environment.SetEnvironmentVariable($"{TestPrefix}GuidValue", "12345678-1234-1234-1234-123456789abc");
            EnvironmentVariableStore.Register(TestPrefix, TestProfileKey);

            // Act
            var settings = MyAppCfg.Get<ITestTypedEnvSettings>();

            // Assert
            Assert.AreEqual(42, settings.IntValue, "Integer value should be parsed correctly from env var");
            Assert.AreEqual(true, settings.BoolValue, "Boolean value should be parsed correctly from env var");
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-123456789abc"), settings.GuidValue, "Guid value should be parsed correctly from env var");
        }

        [Test]
        [Description("Verifies that Register throws ArgumentException when prefix is null")]
        public void Register_WithNullPrefix_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => EnvironmentVariableStore.Register(null));
            Assert.IsNotNull(ex, "ArgumentException should be thrown for null prefix");
        }

        [Test]
        [Description("Verifies that Register throws ArgumentException when prefix is empty string")]
        public void Register_WithEmptyPrefix_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => EnvironmentVariableStore.Register(""));
            Assert.IsNotNull(ex, "ArgumentException should be thrown for empty prefix");
        }

        [Test]
        [Description("Verifies that DefaultOption attribute works correctly with environment variable store")]
        public void LoadFromEnvironmentVariable_DefaultOption_AppliesCorrectly()
        {
            // Arrange
            Environment.SetEnvironmentVariable($"{TestPrefix}ApiKey", "my-api-key");
            EnvironmentVariableStore.Register(TestPrefix, TestProfileKey);

            // Act
            var settings = MyAppCfg.Get<ITestDefaultOptionEnvSettings>();

            // Assert
            Assert.AreEqual("my-api-key", settings.ApiKey, "ApiKey should be loaded using DefaultOption ProfileKey");
        }

        // Test interfaces
        public interface ITestEnvSettings
        {
            [Option(Alias = "TestKey",
                    ProfileKey = TestProfileKey,
                    DefaultValue = "default-value")]
            string TestKey { get; }
        }

        public interface ITestHierarchicalEnvSettings
        {
            [Option(Alias = "Database:Host",
                    ProfileKey = TestProfileKey)]
            string DatabaseHost { get; }

            [Option(Alias = "Database:Port",
                    ProfileKey = TestProfileKey)]
            int DatabasePort { get; }
        }

        public interface ITestTypedEnvSettings
        {
            [Option(Alias = "IntValue",
                    ProfileKey = TestProfileKey)]
            int IntValue { get; }

            [Option(Alias = "BoolValue",
                    ProfileKey = TestProfileKey)]
            bool BoolValue { get; }

            [Option(Alias = "GuidValue",
                    ProfileKey = TestProfileKey)]
            Guid GuidValue { get; }
        }

        [DefaultOption(ProfileKey = TestProfileKey)]
        public interface ITestDefaultOptionEnvSettings
        {
            [Option(Alias = "ApiKey")]
            string ApiKey { get; }
        }
    }
}
