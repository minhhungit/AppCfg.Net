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

            public static void RegisterMsSqlDatabaseStore(string identity, MsSqlSettingStoreConfig store)
            {
                if (store == null)
                {
                    throw new ArgumentException(nameof(store));
                }

                var key = new KeyValuePair<SettingStoreType, string>(SettingStoreType.MsSqlDatabase, identity);
                if (!_settingStore.ContainsKey(key))
                {
                    _settingStore.Add(key, store);
                }
                else
                {
                    _settingStore[key] = store;
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
