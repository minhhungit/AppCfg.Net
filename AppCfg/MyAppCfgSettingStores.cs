using System;
using System.Collections.Generic;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        public class SettingStoreMetadata
        {
            public SettingStoreMetadata(string storeIdentity, string tenantKey, string settingKey, Type typeOfSetting)
            {
                StoreIdentity = storeIdentity;
                TenantKey = tenantKey;
                SettingKey = settingKey;
                TypeOfSetting = typeOfSetting;
            }

            public string StoreIdentity { get; set; }
            public string TenantKey { get; set; }
            public string SettingKey { get; set; }
            public Type TypeOfSetting { get; set; }
        }

        public class SettingStores
        {
            private static readonly IDictionary<KeyValuePair<SettingStoreType, string>, Func<SettingStoreMetadata, string>> _settingStore = new Dictionary<KeyValuePair<SettingStoreType, string>, Func<SettingStoreMetadata, string>>();

            public static void RegisterCustomStore(Func<SettingStoreMetadata, string> getRawValueFunc)
            {
                RegisterCustomStore(null, getRawValueFunc);
            }

            public static void RegisterCustomStore(string storeIdentity, Func<SettingStoreMetadata, string> getRawValueFunc)
            {
                var key = new KeyValuePair<SettingStoreType, string>(SettingStoreType.Custom, storeIdentity);
                if (!_settingStore.ContainsKey(key))
                {
                    _settingStore.Add(key, getRawValueFunc);
                }
                else
                {
                    _settingStore[key] = getRawValueFunc;
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
        }
    }
}
