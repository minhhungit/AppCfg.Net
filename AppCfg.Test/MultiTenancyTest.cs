using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    /// <summary>
    /// Tests for multi-tenancy support - verifying that different tenants receive
    /// isolated configuration values from custom stores.
    /// </summary>
    [TestFixture]
    public class MultiTenancyTest
    {
        private const string TestStoreKey = "MultiTenancy:TestStore";
        private const string AdvancedStoreKey = "MultiTenancy:AdvancedStore";
        private const string TypesStoreKey = "MultiTenancy:TypesStore";

        [SetUp]
        public void Setup()
        {
            // Register a custom store that returns different values based on tenant
            MyAppCfg.SettingStores.RegisterStore(TestStoreKey, metadata =>
            {
                if (metadata.TenantKey == "tenant1")
                {
                    if (metadata.SettingKey == "AppName") return "Tenant1 App";
                    if (metadata.SettingKey == "MaxUsers") return "100";
                }
                else if (metadata.TenantKey == "tenant2")
                {
                    if (metadata.SettingKey == "AppName") return "Tenant2 App";
                    if (metadata.SettingKey == "MaxUsers") return "200";
                }
                else // Default tenant
                {
                    if (metadata.SettingKey == "AppName") return "Default App";
                    if (metadata.SettingKey == "MaxUsers") return "50";
                }

                return null;
            });

            // Register advanced store for more complex scenarios
            MyAppCfg.SettingStores.RegisterStore(AdvancedStoreKey, metadata =>
            {
                var tenant = metadata.TenantKey ?? "";
                var key = metadata.SettingKey;

                // Simulate a database-like store with tenant isolation
                var data = new Dictionary<string, Dictionary<string, string>>
                {
                    [""] = new Dictionary<string, string>
                    {
                        ["ConnectionString"] = "Server=default;Database=app",
                        ["ApiKey"] = "default-api-key",
                        ["MaxRetries"] = "3",
                        ["EnableFeatureX"] = "false",
                        ["Timeout"] = "00:00:30"
                    },
                    ["enterprise"] = new Dictionary<string, string>
                    {
                        ["ConnectionString"] = "Server=enterprise;Database=app;Pooling=true",
                        ["ApiKey"] = "enterprise-api-key-premium",
                        ["MaxRetries"] = "5",
                        ["EnableFeatureX"] = "true",
                        ["Timeout"] = "00:01:00"
                    },
                    ["startup"] = new Dictionary<string, string>
                    {
                        ["ConnectionString"] = "Server=startup;Database=app",
                        ["ApiKey"] = "startup-api-key",
                        ["MaxRetries"] = "2",
                        ["EnableFeatureX"] = "false",
                        ["Timeout"] = "00:00:15"
                    }
                };

                if (data.TryGetValue(tenant, out var tenantData))
                {
                    if (tenantData.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                }

                // Fallback to default tenant
                if (tenant != "" && data.TryGetValue("", out var defaultData))
                {
                    if (defaultData.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                }

                return null;
            });

            // Register store for testing different data types
            MyAppCfg.SettingStores.RegisterStore(TypesStoreKey, metadata =>
            {
                var tenant = metadata.TenantKey ?? "";

                var data = new Dictionary<string, Dictionary<string, string>>
                {
                    [""] = new Dictionary<string, string>
                    {
                        ["StringValue"] = "default-string",
                        ["IntValue"] = "10",
                        ["BoolValue"] = "false",
                        ["DoubleValue"] = "1.5",
                        ["DateTimeValue"] = "2024-01-01",
                        ["GuidValue"] = "11111111-1111-1111-1111-111111111111",
                        ["ListValue"] = "a;b;c"
                    },
                    ["tenantA"] = new Dictionary<string, string>
                    {
                        ["StringValue"] = "tenantA-string",
                        ["IntValue"] = "100",
                        ["BoolValue"] = "true",
                        ["DoubleValue"] = "99.99",
                        ["DateTimeValue"] = "2025-06-15",
                        ["GuidValue"] = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                        ["ListValue"] = "x;y;z"
                    }
                };

                if (data.TryGetValue(tenant, out var tenantData))
                {
                    if (tenantData.TryGetValue(metadata.SettingKey, out var value))
                    {
                        return value;
                    }
                }

                // Fallback
                if (data.TryGetValue("", out var defaultData))
                {
                    if (defaultData.TryGetValue(metadata.SettingKey, out var value))
                    {
                        return value;
                    }
                }

                return null;
            });
        }

        #region Basic Multi-Tenancy Tests

        /// <summary>
        /// Verifies that calling Get without a tenant key returns the default tenant values.
        /// </summary>
        [Test, Description("Get<T>() without tenant key should return default values: AppName='Default App', MaxUsers=50")]
        public void GetSettings_WithoutTenantKey_ReturnsDefaultValues()
        {
            // Act
            var settings = MyAppCfg.Get<ITenantSettings>();

            // Assert
            Assert.AreEqual("Default App", settings.AppName, "AppName should be 'Default App' for default tenant");
            Assert.AreEqual(50, settings.MaxUsers, "MaxUsers should be 50 for default tenant");
        }

        /// <summary>
        /// Verifies that tenant1 receives its specific configuration values.
        /// </summary>
        [Test, Description("Get<T>('tenant1') should return tenant1 values: AppName='Tenant1 App', MaxUsers=100")]
        public void GetSettings_WithTenant1_ReturnsTenant1Values()
        {
            // Act
            var settings = MyAppCfg.Get<ITenantSettings>("tenant1");

            // Assert
            Assert.AreEqual("Tenant1 App", settings.AppName, "AppName should be 'Tenant1 App' for tenant1");
            Assert.AreEqual(100, settings.MaxUsers, "MaxUsers should be 100 for tenant1");
        }

        /// <summary>
        /// Verifies that tenant2 receives its specific configuration values.
        /// </summary>
        [Test, Description("Get<T>('tenant2') should return tenant2 values: AppName='Tenant2 App', MaxUsers=200")]
        public void GetSettings_WithTenant2_ReturnsTenant2Values()
        {
            // Act
            var settings = MyAppCfg.Get<ITenantSettings>("tenant2");

            // Assert
            Assert.AreEqual("Tenant2 App", settings.AppName, "AppName should be 'Tenant2 App' for tenant2");
            Assert.AreEqual(200, settings.MaxUsers, "MaxUsers should be 200 for tenant2");
        }

        /// <summary>
        /// Verifies that different tenants get different isolated values.
        /// </summary>
        [Test, Description("Different tenants (tenant1, tenant2, default) should have different AppName values")]
        public void GetSettings_DifferentTenants_ReturnsDifferentValues()
        {
            // Act
            var tenant1Settings = MyAppCfg.Get<ITenantSettings>("tenant1");
            var tenant2Settings = MyAppCfg.Get<ITenantSettings>("tenant2");
            var defaultSettings = MyAppCfg.Get<ITenantSettings>();

            // Assert
            Assert.AreNotEqual(tenant1Settings.AppName, tenant2Settings.AppName, "tenant1 and tenant2 should differ");
            Assert.AreNotEqual(tenant1Settings.AppName, defaultSettings.AppName, "tenant1 and default should differ");
            Assert.AreNotEqual(tenant2Settings.AppName, defaultSettings.AppName, "tenant2 and default should differ");
        }

        /// <summary>
        /// Verifies that unknown tenants fall back to default tenant values.
        /// </summary>
        [Test, Description("Unknown tenant 'unknown-tenant' should fall back to default values")]
        public void GetSettings_UnknownTenant_ReturnsStoreDefaultValues()
        {
            // Act
            var settings = MyAppCfg.Get<ITenantSettings>("unknown-tenant");

            // Assert
            Assert.AreEqual("Default App", settings.AppName, "Unknown tenant should get default AppName");
            Assert.AreEqual(50, settings.MaxUsers, "Unknown tenant should get default MaxUsers");
        }

        /// <summary>
        /// Verifies that multiple calls to same tenant return consistent values.
        /// </summary>
        [Test, Description("Multiple Get<T>('tenant1') calls should return same values")]
        public void GetSettings_SameTenantMultipleTimes_ReturnsSameValues()
        {
            // Act
            var settings1 = MyAppCfg.Get<ITenantSettings>("tenant1");
            var settings2 = MyAppCfg.Get<ITenantSettings>("tenant1");

            // Assert
            Assert.AreEqual(settings1.AppName, settings2.AppName, "AppName should be consistent across calls");
            Assert.AreEqual(settings1.MaxUsers, settings2.MaxUsers, "MaxUsers should be consistent across calls");
        }

        #endregion

        #region Null and Empty Tenant Key Tests

        /// <summary>
        /// Verifies that null tenant key is treated the same as no tenant (default).
        /// </summary>
        [Test, Description("Get<T>(null) should return same values as Get<T>() without parameter")]
        public void GetSettings_WithNullTenantKey_ReturnsSameAsNoTenant()
        {
            // Act
            var settingsWithNull = MyAppCfg.Get<ITenantSettings>(null);
            var settingsWithoutTenant = MyAppCfg.Get<ITenantSettings>();

            // Assert
            Assert.AreEqual(settingsWithoutTenant.AppName, settingsWithNull.AppName, "Null tenant should match default");
            Assert.AreEqual(settingsWithoutTenant.MaxUsers, settingsWithNull.MaxUsers, "Null tenant should match default");
        }

        /// <summary>
        /// Verifies that empty string tenant key is treated the same as no tenant.
        /// </summary>
        [Test, Description("Get<T>('') empty string should return same values as Get<T>() without parameter")]
        public void GetSettings_WithEmptyStringTenantKey_ReturnsSameAsNoTenant()
        {
            // Act
            var settingsWithEmpty = MyAppCfg.Get<ITenantSettings>("");
            var settingsWithoutTenant = MyAppCfg.Get<ITenantSettings>();

            // Assert
            Assert.AreEqual(settingsWithoutTenant.AppName, settingsWithEmpty.AppName, "Empty tenant should match default");
            Assert.AreEqual(settingsWithoutTenant.MaxUsers, settingsWithEmpty.MaxUsers, "Empty tenant should match default");
        }

        /// <summary>
        /// Verifies that whitespace-only tenant key is treated as unknown tenant (falls back to default).
        /// </summary>
        [Test, Description("Get<T>('   ') whitespace should be treated as unknown tenant (falls back to default)")]
        public void GetSettings_WithWhitespaceTenantKey_TreatedAsUnknownTenant()
        {
            // Act
            var settings = MyAppCfg.Get<ITenantSettings>("   ");

            // Assert
            Assert.AreEqual("Default App", settings.AppName, "Whitespace tenant should fall back to default");
        }

        #endregion

        #region Advanced Store Tests

        /// <summary>
        /// Verifies that default tenant receives base configuration values.
        /// </summary>
        [Test, Description("Default tenant should get base values: ConnectionString=default, MaxRetries=3, Timeout=30s")]
        public void AdvancedStore_DefaultTenant_ReturnsDefaultValues()
        {
            // Act
            var settings = MyAppCfg.Get<IAdvancedTenantSettings>();

            // Assert
            Assert.AreEqual("Server=default;Database=app", settings.ConnectionString, "Default ConnectionString");
            Assert.AreEqual("default-api-key", settings.ApiKey, "Default ApiKey");
            Assert.AreEqual(3, settings.MaxRetries, "Default MaxRetries=3");
            Assert.AreEqual(false, settings.EnableFeatureX, "Default EnableFeatureX=false");
            Assert.AreEqual(TimeSpan.FromSeconds(30), settings.Timeout, "Default Timeout=30s");
        }

        /// <summary>
        /// Verifies that enterprise tenant receives premium configuration values.
        /// </summary>
        [Test, Description("Enterprise tenant should get premium values: MaxRetries=5, EnableFeatureX=true, Timeout=60s")]
        public void AdvancedStore_EnterpriseTenant_ReturnsPremiumValues()
        {
            // Act
            var settings = MyAppCfg.Get<IAdvancedTenantSettings>("enterprise");

            // Assert
            Assert.AreEqual("Server=enterprise;Database=app;Pooling=true", settings.ConnectionString, "Enterprise ConnectionString");
            Assert.AreEqual("enterprise-api-key-premium", settings.ApiKey, "Enterprise ApiKey");
            Assert.AreEqual(5, settings.MaxRetries, "Enterprise MaxRetries=5");
            Assert.AreEqual(true, settings.EnableFeatureX, "Enterprise EnableFeatureX=true");
            Assert.AreEqual(TimeSpan.FromMinutes(1), settings.Timeout, "Enterprise Timeout=60s");
        }

        /// <summary>
        /// Verifies that startup tenant receives limited configuration values.
        /// </summary>
        [Test, Description("Startup tenant should get limited values: MaxRetries=2, EnableFeatureX=false, Timeout=15s")]
        public void AdvancedStore_StartupTenant_ReturnsLimitedValues()
        {
            // Act
            var settings = MyAppCfg.Get<IAdvancedTenantSettings>("startup");

            // Assert
            Assert.AreEqual("Server=startup;Database=app", settings.ConnectionString, "Startup ConnectionString");
            Assert.AreEqual("startup-api-key", settings.ApiKey, "Startup ApiKey");
            Assert.AreEqual(2, settings.MaxRetries, "Startup MaxRetries=2");
            Assert.AreEqual(false, settings.EnableFeatureX, "Startup EnableFeatureX=false");
            Assert.AreEqual(TimeSpan.FromSeconds(15), settings.Timeout, "Startup Timeout=15s");
        }

        /// <summary>
        /// Verifies that unknown tenants fall back to default configuration values.
        /// </summary>
        [Test, Description("Unknown tenant 'nonexistent-tenant' should fall back to default ConnectionString and ApiKey")]
        public void AdvancedStore_UnknownTenant_FallsBackToDefault()
        {
            // Act
            var settings = MyAppCfg.Get<IAdvancedTenantSettings>("nonexistent-tenant");

            // Assert
            Assert.AreEqual("Server=default;Database=app", settings.ConnectionString, "Unknown should fall back to default");
            Assert.AreEqual("default-api-key", settings.ApiKey, "Unknown should fall back to default");
        }

        #endregion

        #region Different Data Types Tests

        /// <summary>
        /// Verifies that all data types (string, int, bool, double, DateTime, Guid, List)
        /// are correctly parsed for default tenant.
        /// </summary>
        [Test, Description("Default tenant should correctly parse all types: string, int=10, bool=false, double=1.5, DateTime, Guid, List")]
        public void TypesStore_DefaultTenant_AllTypesWorkCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<ITypedTenantSettings>();

            // Assert
            Assert.AreEqual("default-string", settings.StringValue, "Default StringValue");
            Assert.AreEqual(10, settings.IntValue, "Default IntValue=10");
            Assert.AreEqual(false, settings.BoolValue, "Default BoolValue=false");
            Assert.AreEqual(1.5, settings.DoubleValue, "Default DoubleValue=1.5");
            Assert.AreEqual(new DateTime(2024, 1, 1), settings.DateTimeValue, "Default DateTimeValue");
            Assert.AreEqual(new Guid("11111111-1111-1111-1111-111111111111"), settings.GuidValue, "Default GuidValue");
            Assert.AreEqual(3, settings.ListValue.Count, "Default ListValue should have 3 items");
            Assert.Contains("a", settings.ListValue, "Default ListValue contains 'a'");
            Assert.Contains("b", settings.ListValue, "Default ListValue contains 'b'");
            Assert.Contains("c", settings.ListValue, "Default ListValue contains 'c'");
        }

        /// <summary>
        /// Verifies that all data types are correctly parsed for tenantA with its specific values.
        /// </summary>
        [Test, Description("TenantA should correctly parse: string, int=100, bool=true, double=99.99, DateTime=2025-06-15")]
        public void TypesStore_TenantA_AllTypesWorkCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<ITypedTenantSettings>("tenantA");

            // Assert
            Assert.AreEqual("tenantA-string", settings.StringValue, "TenantA StringValue");
            Assert.AreEqual(100, settings.IntValue, "TenantA IntValue=100");
            Assert.AreEqual(true, settings.BoolValue, "TenantA BoolValue=true");
            Assert.AreEqual(99.99, settings.DoubleValue, "TenantA DoubleValue=99.99");
            Assert.AreEqual(new DateTime(2025, 6, 15), settings.DateTimeValue, "TenantA DateTimeValue");
            Assert.AreEqual(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), settings.GuidValue, "TenantA GuidValue");
            Assert.AreEqual(3, settings.ListValue.Count, "TenantA ListValue should have 3 items");
            Assert.Contains("x", settings.ListValue, "TenantA ListValue contains 'x'");
            Assert.Contains("y", settings.ListValue, "TenantA ListValue contains 'y'");
            Assert.Contains("z", settings.ListValue, "TenantA ListValue contains 'z'");
        }

        /// <summary>
        /// Verifies that different tenants receive different typed values for all properties.
        /// </summary>
        [Test, Description("Default and TenantA should have different values for all typed properties")]
        public void TypesStore_DifferentTenants_ReturnDifferentTypedValues()
        {
            // Act
            var defaultSettings = MyAppCfg.Get<ITypedTenantSettings>();
            var tenantASettings = MyAppCfg.Get<ITypedTenantSettings>("tenantA");

            // Assert
            Assert.AreNotEqual(defaultSettings.StringValue, tenantASettings.StringValue, "StringValue should differ");
            Assert.AreNotEqual(defaultSettings.IntValue, tenantASettings.IntValue, "IntValue should differ");
            Assert.AreNotEqual(defaultSettings.BoolValue, tenantASettings.BoolValue, "BoolValue should differ");
            Assert.AreNotEqual(defaultSettings.DoubleValue, tenantASettings.DoubleValue, "DoubleValue should differ");
            Assert.AreNotEqual(defaultSettings.DateTimeValue, tenantASettings.DateTimeValue, "DateTimeValue should differ");
            Assert.AreNotEqual(defaultSettings.GuidValue, tenantASettings.GuidValue, "GuidValue should differ");
        }

        #endregion

        #region Default Value Tests

        /// <summary>
        /// Verifies that missing settings use DefaultValue from Option attribute.
        /// </summary>
        [Test, Description("Missing property with DefaultValue='fallback-value' should return 'fallback-value'")]
        public void GetSettings_MissingSettingWithDefaultValue_ReturnsDefaultValue()
        {
            // Act
            var settings = MyAppCfg.Get<ISettingsWithDefaults>("tenant1");

            // Assert
            Assert.AreEqual("fallback-value", settings.MissingProperty, "Missing property should use DefaultValue");
        }

        /// <summary>
        /// Verifies that missing settings without DefaultValue return null.
        /// </summary>
        [Test, Description("Missing property without DefaultValue should return null")]
        public void GetSettings_MissingSettingWithoutDefaultValue_ReturnsNull()
        {
            // Act
            var settings = MyAppCfg.Get<ISettingsWithDefaults>("tenant1");

            // Assert
            Assert.IsNull(settings.NullableProperty, "Missing property without DefaultValue should be null");
        }

        #endregion

        #region Tenant Isolation Tests

        /// <summary>
        /// Verifies that each tenant's values are isolated from other tenants.
        /// </summary>
        [Test, Description("Tenant1 and Tenant2 should have completely isolated values")]
        public void TenantIsolation_ModifyingOneTenantDoesNotAffectOther()
        {
            // Act
            var tenant1Settings = MyAppCfg.Get<ITenantSettings>("tenant1");
            var tenant2Settings = MyAppCfg.Get<ITenantSettings>("tenant2");

            // Assert
            Assert.AreEqual("Tenant1 App", tenant1Settings.AppName, "Tenant1 should have its own AppName");
            Assert.AreEqual("Tenant2 App", tenant2Settings.AppName, "Tenant2 should have its own AppName");
            Assert.AreEqual(100, tenant1Settings.MaxUsers, "Tenant1 should have its own MaxUsers");
            Assert.AreEqual(200, tenant2Settings.MaxUsers, "Tenant2 should have its own MaxUsers");
        }

        /// <summary>
        /// Verifies that multiple calls to the same tenant always return consistent values.
        /// </summary>
        [Test, Description("10 consecutive calls to tenant1 should all return 'Tenant1 App'")]
        public void TenantIsolation_MultipleCallsToSameTenant_ReturnConsistentValues()
        {
            // Act
            var results = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                var settings = MyAppCfg.Get<ITenantSettings>("tenant1");
                results.Add(settings.AppName);
            }

            // Assert
            Assert.IsTrue(results.TrueForAll(r => r == "Tenant1 App"), "All 10 calls should return 'Tenant1 App'");
        }

        /// <summary>
        /// Verifies that alternating between tenants returns correct values each time.
        /// </summary>
        [Test, Description("Alternating calls between tenant1/tenant2/default should return correct values each time")]
        public void TenantIsolation_AlternatingTenantCalls_ReturnCorrectValues()
        {
            // Act & Assert
            for (int i = 0; i < 5; i++)
            {
                var tenant1Settings = MyAppCfg.Get<ITenantSettings>("tenant1");
                var tenant2Settings = MyAppCfg.Get<ITenantSettings>("tenant2");
                var defaultSettings = MyAppCfg.Get<ITenantSettings>();

                Assert.AreEqual("Tenant1 App", tenant1Settings.AppName, $"Iteration {i}: tenant1 should be 'Tenant1 App'");
                Assert.AreEqual("Tenant2 App", tenant2Settings.AppName, $"Iteration {i}: tenant2 should be 'Tenant2 App'");
                Assert.AreEqual("Default App", defaultSettings.AppName, $"Iteration {i}: default should be 'Default App'");
            }
        }

        #endregion

        #region Metadata Tests

        /// <summary>
        /// Verifies that TenantKey is correctly passed to the store in metadata.
        /// </summary>
        [Test, Description("Store should receive TenantKey='my-tenant-123' in metadata")]
        public void Metadata_TenantKeyIsPassedCorrectly()
        {
            // Arrange
            string capturedTenantKey = null;
            MyAppCfg.SettingStores.RegisterStore("MetadataTest", metadata =>
            {
                capturedTenantKey = metadata.TenantKey;
                return "test-value";
            });

            // Act
            MyAppCfg.Get<IMetadataTestSettings>("my-tenant-123");

            // Assert
            Assert.AreEqual("my-tenant-123", capturedTenantKey, "TenantKey should be passed to store");
        }

        /// <summary>
        /// Verifies that SettingKey is correctly passed to the store for each property.
        /// </summary>
        [Test, Description("Store should receive SettingKey for each property: 'TestKey1', 'TestKey2'")]
        public void Metadata_SettingKeyIsPassedCorrectly()
        {
            // Arrange
            var capturedKeys = new List<string>();
            MyAppCfg.SettingStores.RegisterStore("MetadataTest2", metadata =>
            {
                capturedKeys.Add(metadata.SettingKey);
                return "test-value";
            });

            // Act
            MyAppCfg.Get<IMetadataTestSettings2>("tenant");

            // Assert
            Assert.Contains("TestKey1", capturedKeys, "Should capture TestKey1");
            Assert.Contains("TestKey2", capturedKeys, "Should capture TestKey2");
        }

        /// <summary>
        /// Verifies that ProfileKey is correctly passed to the store in metadata.
        /// </summary>
        [Test, Description("Store should receive ProfileKey='ProfileKeyTest' in metadata")]
        public void Metadata_ProfileKeyIsPassedCorrectly()
        {
            // Arrange
            string capturedProfileKey = null;
            MyAppCfg.SettingStores.RegisterStore("ProfileKeyTest", metadata =>
            {
                capturedProfileKey = metadata.ProfileKey;
                return "test-value";
            });

            // Act
            MyAppCfg.Get<IProfileKeyTestSettings>("tenant");

            // Assert
            Assert.AreEqual("ProfileKeyTest", capturedProfileKey, "ProfileKey should be passed to store");
        }

        #endregion

        #region Test Interfaces

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface ITenantSettings
        {
            [Option(Alias = "AppName")]
            string AppName { get; }

            [Option(Alias = "MaxUsers")]
            int MaxUsers { get; }
        }

        [DefaultOption(ProfileKey = AdvancedStoreKey)]
        public interface IAdvancedTenantSettings
        {
            [Option(Alias = "ConnectionString")]
            string ConnectionString { get; }

            [Option(Alias = "ApiKey")]
            string ApiKey { get; }

            [Option(Alias = "MaxRetries")]
            int MaxRetries { get; }

            [Option(Alias = "EnableFeatureX")]
            bool EnableFeatureX { get; }

            [Option(Alias = "Timeout")]
            TimeSpan Timeout { get; }
        }

        [DefaultOption(ProfileKey = TypesStoreKey)]
        public interface ITypedTenantSettings
        {
            [Option(Alias = "StringValue")]
            string StringValue { get; }

            [Option(Alias = "IntValue")]
            int IntValue { get; }

            [Option(Alias = "BoolValue")]
            bool BoolValue { get; }

            [Option(Alias = "DoubleValue")]
            double DoubleValue { get; }

            [Option(Alias = "DateTimeValue")]
            DateTime DateTimeValue { get; }

            [Option(Alias = "GuidValue")]
            Guid GuidValue { get; }

            [Option(Alias = "ListValue")]
            List<string> ListValue { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface ISettingsWithDefaults
        {
            [Option(Alias = "MissingProperty", DefaultValue = "fallback-value")]
            string MissingProperty { get; }

            [Option(Alias = "NullableProperty")]
            string NullableProperty { get; }
        }

        [DefaultOption(ProfileKey = "MetadataTest")]
        public interface IMetadataTestSettings
        {
            [Option(Alias = "TestKey")]
            string TestKey { get; }
        }

        [DefaultOption(ProfileKey = "MetadataTest2")]
        public interface IMetadataTestSettings2
        {
            [Option(Alias = "TestKey1")]
            string TestKey1 { get; }

            [Option(Alias = "TestKey2")]
            string TestKey2 { get; }
        }

        [DefaultOption(ProfileKey = "ProfileKeyTest")]
        public interface IProfileKeyTestSettings
        {
            [Option(Alias = "TestKey")]
            string TestKey { get; }
        }

        #endregion
    }
}
