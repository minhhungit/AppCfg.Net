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
            // Register a simple custom store for testing
            MyAppCfg.SettingStores.RegisterCustomStore(TestStoreId, metadata =>
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
        public void DefaultOption_AllPropertiesUseDefault_LoadsFromCustomStore()
        {
            // Act
            var settings = MyAppCfg.Get<ITestDefaultStoreSettings>();

            // Assert
            Assert.AreEqual("custom-value", settings.FromCustomStore);
            Assert.AreEqual("another-custom", settings.AnotherCustomValue);
        }

        [Test]
        public void DefaultOption_PropertyOverridesStoreType_LoadsFromAppSettings()
        {
            // Act
            var settings = MyAppCfg.Get<ITestMixedStoreSettings>();

            // Assert
            Assert.AreEqual("custom-value", settings.FromCustomStore); // Uses default (custom)
            Assert.AreEqual("29", settings.FromAppSettings); // Overrides to AppSettings
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
        [DefaultOption(StoreType = SettingStoreType.Custom, StoreIdentity = TestStoreId)]
        public interface ITestDefaultStoreSettings
        {
            [Option(Alias = "FromCustomStore")]
            string FromCustomStore { get; }

            [Option(Alias = "AnotherCustomValue")]
            string AnotherCustomValue { get; }

            [Option(Alias = "NonExistentKey", DefaultValue = "fallback")]
            string MissingValue { get; }
        }

        [DefaultOption(StoreType = SettingStoreType.Custom, StoreIdentity = TestStoreId)]
        public interface ITestMixedStoreSettings
        {
            // Uses default (Custom store)
            [Option(Alias = "FromCustomStore")]
            string FromCustomStore { get; }

            // Overrides to use AppSettings (StoreIdentity must be set to override)
            [Option(Alias = "Age", StoreType = SettingStoreType.AppSetting, StoreIdentity = "")]
            string FromAppSettings { get; }
        }

        // Interface without DefaultOption (backward compatibility)
        public interface ITestNoDefaultSettings
        {
            [Option(Alias = "FromCustomStore",
                    StoreType = SettingStoreType.Custom,
                    StoreIdentity = TestStoreId)]
            string ExplicitCustomStore { get; }
        }
    }
}
