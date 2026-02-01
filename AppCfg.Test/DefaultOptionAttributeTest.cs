using AppCfg;
using AppCfg.SettingStore;
using NUnit.Framework;
using System;
using System.Configuration;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Tests for DefaultOptionAttribute functionality at interface level")]
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
        [Description("Verifies that all properties load from the profile store when DefaultOption is set at interface level")]
        public void DefaultOption_AllPropertiesUseDefault_LoadsFromProfileStore()
        {
            // Act
            var settings = MyAppCfg.Get<ITestDefaultStoreSettings>();

            // Assert
            Assert.AreEqual("custom-value", settings.FromCustomStore, "FromCustomStore should load from custom store defined in DefaultOption");
            Assert.AreEqual("another-custom", settings.AnotherCustomValue, "AnotherCustomValue should load from custom store defined in DefaultOption");
        }

        [Test]
        [Description("Verifies that individual property can override the default ProfileKey to load from App.config")]
        public void DefaultOption_PropertyOverridesProfileKey_LoadsFromAppSettings()
        {
            // Act
            var settings = MyAppCfg.Get<ITestMixedStoreSettings>();

            // Assert
            Assert.AreEqual("custom-value", settings.FromCustomStore, "FromCustomStore should use default ProfileKey (TestStoreId)");
            Assert.AreEqual("29", settings.FromAppSettings, "FromAppSettings should override to empty ProfileKey and load from App.config");
        }

        [Test]
        [Description("Verifies backward compatibility when interface does not have DefaultOption attribute")]
        public void DefaultOption_WithoutDefaultOption_UsesPropertySettings()
        {
            // Act - Interface without DefaultOption should work as before
            var settings = MyAppCfg.Get<ITestNoDefaultSettings>();

            // Assert
            Assert.AreEqual("custom-value", settings.ExplicitCustomStore, "Property with explicit ProfileKey should still work without DefaultOption");
        }

        [Test]
        [Description("Verifies that DefaultValue on property is used when custom store returns null")]
        public void DefaultOption_WithDefaultValues_AppliesCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<ITestDefaultStoreSettings>();

            // Assert - Property with no custom store value should use DefaultValue
            Assert.AreEqual("fallback", settings.MissingValue, "MissingValue should use DefaultValue attribute when store returns null");
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
