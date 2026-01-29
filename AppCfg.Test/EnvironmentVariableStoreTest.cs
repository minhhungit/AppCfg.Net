using AppCfg;
using AppCfg.SettingStore;
using NUnit.Framework;
using System;

namespace AppCfg.Test
{
    [TestFixture]
    public class EnvironmentVariableStoreTest
    {
        private const string TestPrefix = "APPCFGTEST__";
        private const string TestStoreIdentity = "EnvironmentVariables:" + TestPrefix;

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
        public void LoadFromEnvironmentVariable_SimpleKey_ReturnsCorrectValue()
        {
            // Arrange
            Environment.SetEnvironmentVariable($"{TestPrefix}TestKey", "test-value");
            EnvironmentVariableStore.Register(TestPrefix, TestStoreIdentity);

            // Act
            var settings = MyAppCfg.Get<ITestEnvSettings>();

            // Assert
            Assert.AreEqual("test-value", settings.TestKey);
        }

        [Test]
        public void LoadFromEnvironmentVariable_HierarchicalKey_ReturnsCorrectValue()
        {
            // Arrange
            Environment.SetEnvironmentVariable($"{TestPrefix}Database__Host", "localhost");
            Environment.SetEnvironmentVariable($"{TestPrefix}Database__Port", "5432");
            EnvironmentVariableStore.Register(TestPrefix, TestStoreIdentity);

            // Act
            var settings = MyAppCfg.Get<ITestHierarchicalEnvSettings>();

            // Assert
            Assert.AreEqual("localhost", settings.DatabaseHost);
            Assert.AreEqual(5432, settings.DatabasePort);
        }

        [Test]
        public void LoadFromEnvironmentVariable_MissingVariable_UsesDefaultValue()
        {
            // Arrange - Don't set environment variable
            EnvironmentVariableStore.Register(TestPrefix, TestStoreIdentity);

            // Act
            var settings = MyAppCfg.Get<ITestEnvSettings>();

            // Assert - Should use default value
            Assert.AreEqual("default-value", settings.TestKey);
        }

        [Test]
        public void LoadFromEnvironmentVariable_EmptyValue_UsesDefaultValue()
        {
            // Arrange
            // Note: Setting env var to empty string ("") actually DELETES it in .NET
            // So this tests that deleted/missing variables use default values
            Environment.SetEnvironmentVariable($"{TestPrefix}TestKey", "");
            EnvironmentVariableStore.Register(TestPrefix, TestStoreIdentity);

            // Act
            var settings = MyAppCfg.Get<ITestEnvSettings>();

            // Assert - Setting to "" deletes the var, so default value is used
            Assert.AreEqual("default-value", settings.TestKey);
        }

        [Test]
        public void LoadFromEnvironmentVariable_WithTypeParsing_ParsesCorrectly()
        {
            // Arrange
            Environment.SetEnvironmentVariable($"{TestPrefix}IntValue", "42");
            Environment.SetEnvironmentVariable($"{TestPrefix}BoolValue", "true");
            Environment.SetEnvironmentVariable($"{TestPrefix}GuidValue", "12345678-1234-1234-1234-123456789abc");
            EnvironmentVariableStore.Register(TestPrefix, TestStoreIdentity);

            // Act
            var settings = MyAppCfg.Get<ITestTypedEnvSettings>();

            // Assert
            Assert.AreEqual(42, settings.IntValue);
            Assert.AreEqual(true, settings.BoolValue);
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-123456789abc"), settings.GuidValue);
        }

        [Test]
        public void Register_WithNullPrefix_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => EnvironmentVariableStore.Register(null));
        }

        [Test]
        public void Register_WithEmptyPrefix_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => EnvironmentVariableStore.Register(""));
        }

        [Test]
        public void LoadFromEnvironmentVariable_DefaultOption_AppliesCorrectly()
        {
            // Arrange
            Environment.SetEnvironmentVariable($"{TestPrefix}ApiKey", "my-api-key");
            EnvironmentVariableStore.Register(TestPrefix, TestStoreIdentity);

            // Act
            var settings = MyAppCfg.Get<ITestDefaultOptionEnvSettings>();

            // Assert
            Assert.AreEqual("my-api-key", settings.ApiKey);
        }

        // Test interfaces
        public interface ITestEnvSettings
        {
            [Option(Alias = "TestKey",
                    StoreType = SettingStoreType.Custom,
                    StoreIdentity = TestStoreIdentity,
                    DefaultValue = "default-value")]
            string TestKey { get; }
        }

        public interface ITestHierarchicalEnvSettings
        {
            [Option(Alias = "Database:Host",
                    StoreType = SettingStoreType.Custom,
                    StoreIdentity = TestStoreIdentity,
                    DefaultValue = "")]
            string DatabaseHost { get; }

            [Option(Alias = "Database:Port",
                    StoreType = SettingStoreType.Custom,
                    StoreIdentity = TestStoreIdentity,
                    DefaultValue = 0)]
            int DatabasePort { get; }
        }

        public interface ITestTypedEnvSettings
        {
            [Option(Alias = "IntValue",
                    StoreType = SettingStoreType.Custom,
                    StoreIdentity = TestStoreIdentity,
                    DefaultValue = 0)]
            int IntValue { get; }

            [Option(Alias = "BoolValue",
                    StoreType = SettingStoreType.Custom,
                    StoreIdentity = TestStoreIdentity,
                    DefaultValue = false)]
            bool BoolValue { get; }

            [Option(Alias = "GuidValue",
                    StoreType = SettingStoreType.Custom,
                    StoreIdentity = TestStoreIdentity)]
            Guid GuidValue { get; }
        }

        [DefaultOption(StoreType = SettingStoreType.Custom,
                       StoreIdentity = TestStoreIdentity)]
        public interface ITestDefaultOptionEnvSettings
        {
            [Option(Alias = "ApiKey", DefaultValue = "")]
            string ApiKey { get; }
        }
    }
}
