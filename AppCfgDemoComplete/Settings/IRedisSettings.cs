using AppCfg;
using AppCfgDemoComplete.CustomStores;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates custom Redis store.
    /// Requires Redis feature to be enabled and store to be registered.
    /// </summary>
    public interface IRedisSettings
    {
        [Option(Alias = "AppName", ProfileKey = RedisStore.StoreKey)]
        string AppName { get; }

        [Option(Alias = "CacheExpiration", ProfileKey = RedisStore.StoreKey)]
        string CacheExpiration { get; }

        [Option(Alias = "MaxRetries", ProfileKey = RedisStore.StoreKey)]
        int MaxRetries { get; }
    }
}
