using System;
using System.Collections.Concurrent;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        public class SettingStoreMetadata
        {
            public SettingStoreMetadata(string profileKey, string tenantKey, string settingKey, Type typeOfSetting)
            {
                ProfileKey = profileKey;
                TenantKey = tenantKey;
                SettingKey = settingKey;
                TypeOfSetting = typeOfSetting;
            }

            public string ProfileKey { get; set; }
            public string TenantKey { get; set; }
            public string SettingKey { get; set; }
            public Type TypeOfSetting { get; set; }
        }

        public class SettingStores
        {
            private static readonly ConcurrentDictionary<string, Func<SettingStoreMetadata, string>> _settingStore
                = new ConcurrentDictionary<string, Func<SettingStoreMetadata, string>>();

            public static void RegisterStore(string profileKey, Func<SettingStoreMetadata, string> getRawValueFunc)
            {
                if (string.IsNullOrEmpty(profileKey))
                {
                    throw new ArgumentException("Profile key cannot be null or empty.", nameof(profileKey));
                }

                if (getRawValueFunc == null)
                {
                    throw new ArgumentNullException(nameof(getRawValueFunc), "getRawValueFunc cannot be null.");
                }

                _settingStore[profileKey] = getRawValueFunc;
            }

            internal static object Get(string profileKey)
            {
                if (string.IsNullOrEmpty(profileKey))
                {
                    return null;
                }

                return _settingStore.TryGetValue(profileKey, out var store) ? store : null;
            }
        }
    }
}
