using AppCfg;
using AppCfg.SettingStore;
using NUnit.Framework;
using System;
using System.Configuration;

namespace AppCfg.Test
{
    [TestFixture]
    public class DefaultOptionAttributeTest
    {
        private const string TestStoreId = "TestStore";

        [SetUp]
        public void Setup()
        {
            // Register a simple profile store for testing
            MyAppCfg.SettingStores.RegisterStore(TestStoreId, metadata =>
            {
                // Return test values based on key
                switch (metadata.SettingKey)
                {
                    case "FromCustomStore":
                        return "custom-value";
                    case "AnotherCustomValue":
                        return "another-custom";
                    default:
                        return null;
                }
            });
        }

        [Test]
        public void DefaultOption_AllPropertiesUseDefault_LoadsFromProfileStore()
        {
            // Act
            var settings = MyAppCfg.Get<ITestDefaultStoreSettings>();

            // Assert
            Assert.AreEqual("custom-value", settings.FromCustomStore);
            Assert.AreEqual("another-custom", settings.AnotherCustomValue);
        }

        [Test]
        public void DefaultOption_PropertyOverridesProfileKey_LoadsFromAppSettings()
        {
            // Act
            var settings = MyAppCfg.Get<ITestMixedStoreSettings>();

            // Assert
            Assert.AreEqual("custom-value", settings.FromCustomStore); // Uses default (TestStoreId)
            Assert.AreEqual("29", settings.FromAppSettings); // Overrides to App.config
        }

        [Test]
        public void DefaultOption_WithoutDefaultOption_UsesPropertySettings()
        {
            // Act - Interface without DefaultOption should work as before
            var settings = MyAppCfg.Get<ITestNoDefaultSettings>();

            // Assert
            Assert.AreEqual("custom-value", settings.ExplicitCustomStore);
        }

        [Test]
        public void DefaultOption_WithDefaultValues_AppliesCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<ITestDefaultStoreSettings>();

            // Assert - Property with no custom store value should use DefaultValue
            Assert.AreEqual("fallback", settings.MissingValue);
        }

        // Test interfaces
        [DefaultOption(ProfileKey = TestStoreId)]
        public interface ITestDefaultStoreSettings
        {
            [Option(Alias = "FromCustomStore")]
            string FromCustomStore { get; }

            [Option(Alias = "AnotherCustomValue")]
            string AnotherCustomValue { get; }

            [Option(Alias = "NonExistentKey", DefaultValue = "fallback")]
            string MissingValue { get; }
        }

        [DefaultOption(ProfileKey = TestStoreId)]
        public interface ITestMixedStoreSettings
        {
            // Uses default (custom store)
            [Option(Alias = "FromCustomStore")]
            string FromCustomStore { get; }

            // Overrides to use AppSettings (ProfileKey = "")
            [Option(Alias = "Age", ProfileKey = "")]
            string FromAppSettings { get; }
        }

        // Interface without DefaultOption (backward compatibility)
        public interface ITestNoDefaultSettings
        {
            [Option(Alias = "FromCustomStore", ProfileKey = TestStoreId)]
            string ExplicitCustomStore { get; }
        }
    }
}
