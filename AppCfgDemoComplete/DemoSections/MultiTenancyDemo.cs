using AppCfg;
using AppCfgDemoComplete.CustomStores;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class MultiTenancyDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Multi-Tenancy Demo",
                "Demonstrates retrieving different configuration values for different tenants.\n" +
                "Perfect for SaaS applications with tenant-specific settings.\n" +
                "Use MyAppCfg.Get<T>(tenantKey) to specify the tenant.\n\n" +
                "IMPORTANT: Multi-tenancy requires a custom store (Database, Redis, In-Memory, etc.)"
            );

            try
            {
                // Initialize the tenant store - REQUIRED for multi-tenancy
                MySettings.InitializeTenantStore();

                OutputHelper.WriteInfo($"Tenant store registered: {InMemoryTenantStore.StoreKey}");
                Console.WriteLine();

                // Get settings without tenant (default tenant)
                var defaultSettings = MyAppCfg.Get<IMultiTenantSettings>();

                OutputHelper.WriteHeader("Default Tenant (No Tenant Key)");
                OutputHelper.WriteSetting("AppName", defaultSettings.AppName, "InMemoryTenantStore");
                OutputHelper.WriteSetting("MaxConnections", defaultSettings.MaxConnections, "InMemoryTenantStore");
                OutputHelper.WriteSetting("Theme", defaultSettings.Theme, "InMemoryTenantStore");
                OutputHelper.WriteSetting("ApiEndpoint", defaultSettings.ApiEndpoint, "InMemoryTenantStore");

                Console.WriteLine();

                // Get settings for tenant-1 (Enterprise customer)
                var tenant1Settings = MyAppCfg.Get<IMultiTenantSettings>("tenant-1");

                OutputHelper.WriteHeader("Tenant: 'tenant-1' (Enterprise)");
                OutputHelper.WriteSetting("AppName", tenant1Settings.AppName, "InMemoryTenantStore");
                OutputHelper.WriteSetting("MaxConnections", tenant1Settings.MaxConnections, "InMemoryTenantStore");
                OutputHelper.WriteSetting("Theme", tenant1Settings.Theme, "InMemoryTenantStore");
                OutputHelper.WriteSetting("ApiEndpoint", tenant1Settings.ApiEndpoint, "InMemoryTenantStore");

                Console.WriteLine();

                // Get settings for tenant-2 (Small business)
                var tenant2Settings = MyAppCfg.Get<IMultiTenantSettings>("tenant-2");

                OutputHelper.WriteHeader("Tenant: 'tenant-2' (Small Business)");
                OutputHelper.WriteSetting("AppName", tenant2Settings.AppName, "InMemoryTenantStore");
                OutputHelper.WriteSetting("MaxConnections", tenant2Settings.MaxConnections, "InMemoryTenantStore");
                OutputHelper.WriteSetting("Theme", tenant2Settings.Theme, "InMemoryTenantStore");
                OutputHelper.WriteSetting("ApiEndpoint", tenant2Settings.ApiEndpoint, "InMemoryTenantStore");

                Console.WriteLine();

                // Demonstrate unknown tenant fallback
                var unknownSettings = MyAppCfg.Get<IMultiTenantSettings>("unknown-tenant");

                OutputHelper.WriteHeader("Unknown Tenant (falls back to default)");
                OutputHelper.WriteSetting("AppName", unknownSettings.AppName, "Fallback to default");

                Console.WriteLine();
                OutputHelper.WriteSection("How Multi-Tenancy Works");

                Console.WriteLine("1. Register a custom store that handles tenant-specific data:");
                Console.WriteLine("   MyAppCfg.SettingStores.RegisterStore(\"MyStore\", metadata => {");
                Console.WriteLine("       var tenant = metadata.TenantKey;  // <-- tenant key passed here");
                Console.WriteLine("       return GetValueForTenant(tenant, metadata.SettingKey);");
                Console.WriteLine("   });");
                Console.WriteLine();

                Console.WriteLine("2. Use [DefaultOption] on your interface to specify the store:");
                Console.WriteLine("   [DefaultOption(ProfileKey = \"MyStore\")]");
                Console.WriteLine("   public interface IMultiTenantSettings { ... }");
                Console.WriteLine();

                Console.WriteLine("3. Call Get<T>(tenantKey) to retrieve tenant-specific settings:");
                Console.WriteLine("   var settings = MyAppCfg.Get<IMultiTenantSettings>(\"tenant-1\");");
                Console.WriteLine();

                OutputHelper.WriteSuccess("Multi-tenancy enables SaaS applications with per-customer configuration!");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run MultiTenancy demo: {ex.Message}");
                Console.WriteLine($"Details: {ex}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
