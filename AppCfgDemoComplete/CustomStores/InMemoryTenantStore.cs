using AppCfg;
using System;
using System.Collections.Generic;

namespace AppCfgDemoComplete.CustomStores
{
    /// <summary>
    /// Simple in-memory tenant store for demonstration purposes.
    /// In production, you would use a database, Redis, or other persistent storage.
    /// </summary>
    public static class InMemoryTenantStore
    {
        public const string StoreKey = "InMemoryTenant";

        // Simulated tenant-specific configuration data
        private static readonly Dictionary<string, Dictionary<string, string>> TenantData =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Default tenant (no tenant key or null)
                [""] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["AppName"] = "MyApp (Default Tenant)",
                    ["MaxConnections"] = "100",
                    ["Theme"] = "Light",
                    ["ApiEndpoint"] = "https://api.default.example.com"
                },
                // Tenant 1 - Enterprise customer
                ["tenant-1"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["AppName"] = "MyApp Enterprise (Tenant-1)",
                    ["MaxConnections"] = "500",
                    ["Theme"] = "Dark",
                    ["ApiEndpoint"] = "https://api.tenant1.example.com"
                },
                // Tenant 2 - Small business customer
                ["tenant-2"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["AppName"] = "MyApp Basic (Tenant-2)",
                    ["MaxConnections"] = "50",
                    ["Theme"] = "Blue",
                    ["ApiEndpoint"] = "https://api.tenant2.example.com"
                }
            };

        /// <summary>
        /// Register the in-memory tenant store with AppCfg.
        /// </summary>
        public static void Register()
        {
            MyAppCfg.SettingStores.RegisterStore(StoreKey, metadata =>
            {
                // Use empty string for null/empty tenant key (default tenant)
                var tenantKey = string.IsNullOrEmpty(metadata.TenantKey) ? "" : metadata.TenantKey;

                // Try to get tenant-specific data
                if (TenantData.TryGetValue(tenantKey, out var tenantSettings))
                {
                    if (tenantSettings.TryGetValue(metadata.SettingKey, out var value))
                    {
                        return value;
                    }
                }

                // Fallback to default tenant if specific tenant not found
                if (tenantKey != "" && TenantData.TryGetValue("", out var defaultSettings))
                {
                    if (defaultSettings.TryGetValue(metadata.SettingKey, out var value))
                    {
                        return value;
                    }
                }

                // Return null to allow DefaultValue attribute to work
                return null;
            });
        }

        /// <summary>
        /// Get all available tenant keys for display purposes.
        /// </summary>
        public static IEnumerable<string> GetAvailableTenants()
        {
            return TenantData.Keys;
        }
    }
}
