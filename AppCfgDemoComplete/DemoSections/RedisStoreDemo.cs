using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class RedisStoreDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Redis Store Demo - REGISTRATION API",
                "Demonstrates custom Redis store for configuration.\n" +
                "Shows how to REGISTER custom stores using MyAppCfg.SettingStores.RegisterStore().\n" +
                "Redis is great for distributed configuration and caching."
            );

            if (!MySettings.IsRedisEnabled())
            {
                MenuHelper.ShowFeatureDisabledMessage("Redis Store", "Demo:EnableRedis");
                Console.WriteLine("Additional setup required:");
                Console.WriteLine("  1. Install Redis server");
                Console.WriteLine("  2. Start Redis service");
                Console.WriteLine("  3. Install StackExchange.Redis NuGet package");
                Console.WriteLine("  4. Update connection string in App.config");
                Console.WriteLine();
                Console.WriteLine("See CustomStores/RedisStore.cs for detailed setup instructions.");
                MenuHelper.PauseForUser();
                return;
            }

            try
            {
                MySettings.InitializeRedisStore();

                if (!MySettings.IsRedisInitialized())
                {
                    OutputHelper.WriteError("Redis store initialization failed!");
                    OutputHelper.WriteWarning("Check your connection string and ensure Redis is running.");
                    MenuHelper.PauseForUser();
                    return;
                }

                OutputHelper.WriteSuccess("Redis store registered successfully!");
                Console.WriteLine();

                // Get settings without tenant
                var settingsNoTenant = MyAppCfg.Get<IRedisSettings>();

                OutputHelper.WriteHeader("Settings Without Tenant");
                OutputHelper.WriteSetting("AppName", settingsNoTenant.AppName, "Redis");
                OutputHelper.WriteSetting("CacheExpiration", settingsNoTenant.CacheExpiration, "Redis");
                OutputHelper.WriteSetting("MaxRetries", settingsNoTenant.MaxRetries, "Redis");

                Console.WriteLine();

                // Get settings with tenant
                var settingsWithTenant = MyAppCfg.Get<IRedisSettings>("tenant-1");

                OutputHelper.WriteHeader("Settings With Tenant 'tenant-1'");
                OutputHelper.WriteSetting("AppName", settingsWithTenant.AppName, "Redis");
                OutputHelper.WriteSetting("CacheExpiration", settingsWithTenant.CacheExpiration, "Redis");
                OutputHelper.WriteSetting("MaxRetries", settingsWithTenant.MaxRetries, "Redis");

                Console.WriteLine();
                OutputHelper.WriteInfo("Redis is perfect for distributed systems and microservices!");
                Console.WriteLine();
                Console.WriteLine("Benefits:");
                Console.WriteLine("  ✓ Centralized configuration");
                Console.WriteLine("  ✓ Real-time updates");
                Console.WriteLine("  ✓ Distributed caching");
                Console.WriteLine("  ✓ High performance");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Redis error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Common issues:");
                Console.WriteLine("  - Redis server not running");
                Console.WriteLine("  - Connection string incorrect");
                Console.WriteLine("  - Firewall blocking connection");
            }

            MenuHelper.PauseForUser();
        }
    }
}
