using AppCfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    /// <summary>
    /// Tests for custom setting stores - registration, lookup, error handling, and advanced scenarios.
    /// </summary>
    [TestFixture]
    public class CustomStoreTest
    {
        private int _callCount;
        private List<MyAppCfg.SettingStoreMetadata> _capturedMetadata;

        [SetUp]
        public void Setup()
        {
            _callCount = 0;
            _capturedMetadata = new List<MyAppCfg.SettingStoreMetadata>();
        }

        #region Store Registration Tests

        /// <summary>
        /// Verifies that a registered store can be used to retrieve configuration values.
        /// </summary>
        [Test, Description("Registered store should be usable via MyAppCfg.Get<T>() to retrieve values")]
        public void RegisterStore_ValidStore_CanBeUsed()
        {
            // Arrange
            const string storeKey = "TestStore:Valid";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata => "test-value");

            // Act
            var settings = MyAppCfg.Get<IValidStoreTestSettings>();

            // Assert
            Assert.AreEqual("test-value", settings.TestKey, "Store should return the registered value");
        }

        /// <summary>
        /// Verifies that registering a store with the same key twice replaces the previous store.
        /// </summary>
        [Test, Description("Registering same key twice should overwrite previous store with new one")]
        public void RegisterStore_SameKeyTwice_OverwritesPrevious()
        {
            // Arrange
            const string storeKey = "TestStore:Overwrite";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata => "first-value");
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata => "second-value");

            // Act
            var settings = MyAppCfg.Get<IOverwriteTestSettings>();

            // Assert
            Assert.AreEqual("second-value", settings.TestKey, "Second registration should overwrite first");
        }

        /// <summary>
        /// Verifies that accessing a non-existent store throws AppCfgException with descriptive message.
        /// </summary>
        [Test, Description("Using non-existent store key should throw AppCfgException")]
        public void GetStore_NonExistentKey_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<INonExistentStoreSettings>();
            });

            Assert.That(ex.Message, Does.Contain("No store registered"),
                "Exception should indicate missing store registration");
        }

        /// <summary>
        /// Verifies that RegisterStore throws ArgumentException when profile key is null.
        /// </summary>
        [Test, Description("RegisterStore with null key should throw ArgumentException")]
        public void RegisterStore_WithNullKey_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                MyAppCfg.SettingStores.RegisterStore(null, metadata => "value");
            });

            Assert.That(ex.ParamName, Is.EqualTo("profileKey"), "Exception should identify profileKey parameter");
        }

        /// <summary>
        /// Verifies that RegisterStore throws ArgumentException when profile key is empty string.
        /// </summary>
        [Test, Description("RegisterStore with empty key should throw ArgumentException")]
        public void RegisterStore_WithEmptyKey_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                MyAppCfg.SettingStores.RegisterStore("", metadata => "value");
            });

            Assert.That(ex.ParamName, Is.EqualTo("profileKey"), "Exception should identify profileKey parameter");
        }

        /// <summary>
        /// Verifies that RegisterStore throws ArgumentNullException when function is null.
        /// </summary>
        [Test, Description("RegisterStore with null function should throw ArgumentNullException")]
        public void RegisterStore_WithNullFunc_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                MyAppCfg.SettingStores.RegisterStore("TestStore:NullFunc", null);
            });

            Assert.That(ex.ParamName, Is.EqualTo("getRawValueFunc"), "Exception should identify function parameter");
        }

        #endregion

        #region Store Invocation Tests

        /// <summary>
        /// Verifies that store function is called once per property during MyAppCfg.Get<T>(),
        /// not on each property access.
        /// </summary>
        [Test, Description("Store should be called once per property during Get<T>(), not on each property access")]
        public void Store_IsCalledOncePerProperty()
        {
            // Arrange
            const string storeKey = "TestStore:CallCount";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata =>
            {
                _callCount++;
                return "value";
            });

            // Act
            var settings = MyAppCfg.Get<ICallCountTestSettings>();
            var _ = settings.Property1;
            var __ = settings.Property2;

            // Assert
            Assert.AreEqual(2, _callCount, "Store should be called exactly twice (once per property)");
        }

        /// <summary>
        /// Verifies that store receives correct metadata including SettingKey, TenantKey, and ProfileKey.
        /// </summary>
        [Test, Description("Store should receive correct SettingKey, TenantKey, and ProfileKey in metadata")]
        public void Store_ReceivesCorrectMetadata()
        {
            // Arrange
            const string storeKey = "TestStore:Metadata";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata =>
            {
                _capturedMetadata.Add(metadata);
                return "value";
            });

            // Act
            var settings = MyAppCfg.Get<IMetadataTestSettings>("test-tenant");

            // Assert
            Assert.AreEqual(1, _capturedMetadata.Count, "Should capture metadata for one property");
            Assert.AreEqual("TestAlias", _capturedMetadata[0].SettingKey, "SettingKey should match Option.Alias");
            Assert.AreEqual("test-tenant", _capturedMetadata[0].TenantKey, "TenantKey should match passed tenant");
            Assert.AreEqual(storeKey, _capturedMetadata[0].ProfileKey, "ProfileKey should match store key");
        }

        /// <summary>
        /// Verifies that when store returns null, DefaultValue from Option attribute is used.
        /// </summary>
        [Test, Description("When store returns null, DefaultValue should be used if specified")]
        public void Store_ReturnsNull_DefaultValueIsUsed()
        {
            // Arrange
            const string storeKey = "TestStore:NullReturn";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata => null);

            // Act
            var settings = MyAppCfg.Get<INullReturnTestSettings>();

            // Assert
            Assert.AreEqual("default-value", settings.WithDefault, "Property with DefaultValue should use it");
            Assert.IsNull(settings.WithoutDefault, "Property without DefaultValue should be null");
        }

        /// <summary>
        /// Verifies that exceptions thrown by store are wrapped in AppCfgException.
        /// </summary>
        [Test, Description("Exception from store should be wrapped in AppCfgException")]
        public void Store_ThrowsException_WrappedInAppCfgException()
        {
            // Arrange
            const string storeKey = "TestStore:Throws";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata =>
            {
                throw new InvalidOperationException("Test exception");
            });

            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IThrowingStoreSettings>();
            });

            Assert.That(ex.Message, Does.Contain("Test exception"),
                "AppCfgException should contain original exception message");
        }

        #endregion

        #region Multiple Stores Tests

        /// <summary>
        /// Verifies that different interfaces with different ProfileKeys use their respective stores.
        /// </summary>
        [Test, Description("Different interfaces should use their respective stores based on ProfileKey")]
        public void MultipleStores_DifferentInterfaces_UseCorrectStores()
        {
            // Arrange
            MyAppCfg.SettingStores.RegisterStore("Store:A", metadata => "value-from-A");
            MyAppCfg.SettingStores.RegisterStore("Store:B", metadata => "value-from-B");

            // Act
            var settingsA = MyAppCfg.Get<IStoreASettings>();
            var settingsB = MyAppCfg.Get<IStoreBSettings>();

            // Assert
            Assert.AreEqual("value-from-A", settingsA.Value, "Interface A should use Store:A");
            Assert.AreEqual("value-from-B", settingsB.Value, "Interface B should use Store:B");
        }

        /// <summary>
        /// Verifies that properties within same interface can use different stores via ProfileKey on Option.
        /// </summary>
        [Test, Description("Properties with different ProfileKey should use different stores")]
        public void MixedStores_SameInterface_UsesPropertyProfileKey()
        {
            // Arrange
            MyAppCfg.SettingStores.RegisterStore("Store:Primary", metadata => "primary-value");
            MyAppCfg.SettingStores.RegisterStore("Store:Secondary", metadata => "secondary-value");

            // Act
            var settings = MyAppCfg.Get<IMixedStoreSettings>();

            // Assert
            Assert.AreEqual("primary-value", settings.PrimaryValue, "Property should use Store:Primary");
            Assert.AreEqual("secondary-value", settings.SecondaryValue, "Property should use Store:Secondary");
        }

        #endregion

        #region Dynamic Store Tests

        /// <summary>
        /// Verifies that store value changes are reflected in subsequent MyAppCfg.Get<T>() calls.
        /// </summary>
        [Test, Description("Changing store values should be reflected in new Get<T>() calls")]
        public void DynamicStore_ChangesValues_ReflectedInNewGet()
        {
            // Arrange
            var currentValue = "initial";
            const string storeKey = "TestStore:Dynamic";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata => currentValue);

            // Act
            var settings1 = MyAppCfg.Get<IDynamicStoreSettings>();
            currentValue = "changed";
            var settings2 = MyAppCfg.Get<IDynamicStoreSettings>();

            // Assert
            Assert.AreEqual("initial", settings1.Value, "First Get should return initial value");
            Assert.AreEqual("changed", settings2.Value, "Second Get should return changed value");
        }

        /// <summary>
        /// Verifies that store can return different values based on SettingKey from metadata.
        /// </summary>
        [Test, Description("Store can use SettingKey to return different values per property")]
        public void Store_WithConditionalLogic_WorksCorrectly()
        {
            // Arrange
            const string storeKey = "TestStore:Conditional";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata =>
            {
                switch (metadata.SettingKey)
                {
                    case "Key1": return "Value1";
                    case "Key2": return "Value2";
                    case "Key3": return "Value3";
                    default: return "Unknown";
                }
            });

            // Act
            var settings = MyAppCfg.Get<IConditionalStoreSettings>();

            // Assert
            Assert.AreEqual("Value1", settings.Key1, "Key1 should return Value1");
            Assert.AreEqual("Value2", settings.Key2, "Key2 should return Value2");
            Assert.AreEqual("Value3", settings.Key3, "Key3 should return Value3");
            Assert.AreEqual("Unknown", settings.Key4, "Unknown key should return Unknown");
        }

        #endregion

        #region Store with Type Conversion Tests

        /// <summary>
        /// Verifies that string values from store are converted to appropriate types (int, bool, double).
        /// </summary>
        [Test, Description("String values from store should be converted to int, bool, double types")]
        public void Store_ReturnsStringForInt_ParsesCorrectly()
        {
            // Arrange
            const string storeKey = "TestStore:TypeConversion";
            MyAppCfg.SettingStores.RegisterStore(storeKey, metadata =>
            {
                if (metadata.SettingKey == "IntValue") return "42";
                if (metadata.SettingKey == "BoolValue") return "true";
                if (metadata.SettingKey == "DoubleValue") return "3.14";
                return null;
            });

            // Act
            var settings = MyAppCfg.Get<ITypeConversionSettings>();

            // Assert
            Assert.AreEqual(42, settings.IntValue, "String '42' should be parsed as int 42");
            Assert.AreEqual(true, settings.BoolValue, "String 'true' should be parsed as bool true");
            Assert.AreEqual(3.14, settings.DoubleValue, 0.001, "String '3.14' should be parsed as double 3.14");
        }

        #endregion

        #region Test Interfaces

        [DefaultOption(ProfileKey = "TestStore:Valid")]
        public interface IValidStoreTestSettings
        {
            [Option(Alias = "TestKey")]
            string TestKey { get; }
        }

        [DefaultOption(ProfileKey = "NonExistent:Store:Key")]
        public interface INonExistentStoreSettings
        {
            [Option(Alias = "TestKey")]
            string TestKey { get; }
        }

        [DefaultOption(ProfileKey = "TestStore:Overwrite")]
        public interface IOverwriteTestSettings
        {
            [Option(Alias = "TestKey")]
            string TestKey { get; }
        }

        [DefaultOption(ProfileKey = "TestStore:CallCount")]
        public interface ICallCountTestSettings
        {
            [Option(Alias = "Property1")]
            string Property1 { get; }

            [Option(Alias = "Property2")]
            string Property2 { get; }
        }

        [DefaultOption(ProfileKey = "TestStore:Metadata")]
        public interface IMetadataTestSettings
        {
            [Option(Alias = "TestAlias")]
            string TestProperty { get; }
        }

        [DefaultOption(ProfileKey = "TestStore:NullReturn")]
        public interface INullReturnTestSettings
        {
            [Option(Alias = "WithDefault", DefaultValue = "default-value")]
            string WithDefault { get; }

            [Option(Alias = "WithoutDefault")]
            string WithoutDefault { get; }
        }

        [DefaultOption(ProfileKey = "TestStore:Throws")]
        public interface IThrowingStoreSettings
        {
            [Option(Alias = "TestKey")]
            string TestKey { get; }
        }

        [DefaultOption(ProfileKey = "Store:A")]
        public interface IStoreASettings
        {
            [Option(Alias = "Value")]
            string Value { get; }
        }

        [DefaultOption(ProfileKey = "Store:B")]
        public interface IStoreBSettings
        {
            [Option(Alias = "Value")]
            string Value { get; }
        }

        public interface IMixedStoreSettings
        {
            [Option(Alias = "Value", ProfileKey = "Store:Primary")]
            string PrimaryValue { get; }

            [Option(Alias = "Value", ProfileKey = "Store:Secondary")]
            string SecondaryValue { get; }
        }

        [DefaultOption(ProfileKey = "TestStore:Dynamic")]
        public interface IDynamicStoreSettings
        {
            [Option(Alias = "Value")]
            string Value { get; }
        }

        [DefaultOption(ProfileKey = "TestStore:Conditional")]
        public interface IConditionalStoreSettings
        {
            [Option(Alias = "Key1")]
            string Key1 { get; }

            [Option(Alias = "Key2")]
            string Key2 { get; }

            [Option(Alias = "Key3")]
            string Key3 { get; }

            [Option(Alias = "Key4")]
            string Key4 { get; }
        }

        [DefaultOption(ProfileKey = "TestStore:TypeConversion")]
        public interface ITypeConversionSettings
        {
            [Option(Alias = "IntValue")]
            int IntValue { get; }

            [Option(Alias = "BoolValue")]
            bool BoolValue { get; }

            [Option(Alias = "DoubleValue")]
            double DoubleValue { get; }
        }

        #endregion
    }
}
