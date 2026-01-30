using AppCfg;
using System;

namespace AppCfgDemoComplete.CustomStores
{
    /// <summary>
    /// Demonstrates custom Redis store registration.
    /// This is a simplified implementation for demonstration purposes.
    /// In production, use StackExchange.Redis or similar library.
    /// </summary>
    public static class RedisStore
    {
        public const string StoreKey = "Redis > Demo Store";

        /// <summary>
        /// Register custom Redis store.
        /// Demonstrates the REGISTRATION API for custom stores.
        /// </summary>
        public static void Register()
        {
            MyAppCfg.SettingStores.RegisterStore(StoreKey, opt =>
            {
                // Redis key pattern: AppCfgNet_{ProfileKey}_{TenantKey}_{SettingKey}
                var redisKeyPrefix = "AppCfgNet_";
                var tenantPart = string.IsNullOrEmpty(opt.TenantKey) ? "default" : opt.TenantKey;
                var redisKey = $"{redisKeyPrefix}{opt.ProfileKey}_{tenantPart}_{opt.SettingKey}";

                // In a real implementation, you would:
                // 1. Get connection from ConfigurationManager
                // 2. Use StackExchange.Redis to connect
                // 3. Execute GET command
                // 4. Return the value
                //
                // Example with StackExchange.Redis:
                // var connString = ConfigurationManager.ConnectionStrings["DemoRedis"].ConnectionString;
                // var redis = ConnectionMultiplexer.Connect(connString);
                // var db = redis.GetDatabase();
                // var value = db.StringGet(redisKey);
                // return value.HasValue ? (string)value : null;

                // For demo purposes, simulate Redis response
                return $"[Redis Value] Key={redisKey} (Simulated at {DateTime.Now:HH:mm:ss})";
            });
        }
    }
}

/* Redis Setup Instructions:

1. Install Redis:
   - Windows: Download from https://github.com/microsoftarchive/redis/releases
   - Linux: sudo apt-get install redis-server
   - Mac: brew install redis

2. Start Redis:
   - Windows: redis-server.exe
   - Linux/Mac: redis-server

3. Add NuGet package to project:
   Install-Package StackExchange.Redis

4. Test Redis connection:
   redis-cli
   > PING
   > SET AppCfgNet_Redis_default_AppName "MyRedisApp"
   > GET AppCfgNet_Redis_default_AppName

5. Sample data for demo:
   SET AppCfgNet_Redis > Demo Store_default_AppName "MyApp from Redis"
   SET AppCfgNet_Redis > Demo Store_default_CacheExpiration "00:10:00"
   SET AppCfgNet_Redis > Demo Store_default_MaxRetries "5"
   SET AppCfgNet_Redis > Demo Store_tenant-1_AppName "TenantApp from Redis"

*/
