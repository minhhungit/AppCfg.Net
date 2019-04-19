using AppCfg.SettingStore;
using System;
using System.Collections.Generic;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        public class SettingStores
        {
            private static readonly IDictionary<KeyValuePair<SettingStoreType, string>, object> _settingStore;

            static SettingStores()
            {
                _settingStore = new Dictionary<KeyValuePair<SettingStoreType, string>, object>();
            }

            public static void RegisterMsSqlDatabaseStore(string identity, MsSqlSettingStoreConfig config)
            {
                if (config == null)
                {
                    throw new ArgumentException(nameof(config));
                }

                var key = new KeyValuePair<SettingStoreType, string>(SettingStoreType.MsSqlDatabase, identity);
                if (!_settingStore.ContainsKey(key))
                {
                    _settingStore.Add(key, config);
                }
                else
                {
                    _settingStore[key] = config;
                }
            }

            public static void RegisterRedisDatabaseStore(string identity, RedisSettingStoreConfig config)
            {
                if (config == null)
                {
                    throw new ArgumentException(nameof(config));
                }

                var key = new KeyValuePair<SettingStoreType, string>(SettingStoreType.Redis, identity);
                if (!_settingStore.ContainsKey(key))
                {
                    _settingStore.Add(key, config);
                }
                else
                {
                    _settingStore[key] = config;
                }
            }

            internal static object Get(SettingStoreType type, string identity)
            {
                var key = new KeyValuePair<SettingStoreType, string>(type, identity);
                if (!_settingStore.ContainsKey(key))
                {
                    return null;
                }

                return _settingStore[key];
            }

            internal static string GetFirstIdentity(SettingStoreType type)
            {
                foreach (var item in _settingStore)
                {
                    if (item.Key.Key == type)
                    {
                        return item.Key.Value;
                    }
                }

                return null;
            }
        }
    }
}
