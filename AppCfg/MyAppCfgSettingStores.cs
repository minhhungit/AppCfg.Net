using AppCfg.SettingStore;
using System.Collections.Generic;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        public class SettingStores
        {
            private static readonly IDictionary<StoresSupportedType, object> _settingStore;

            static SettingStores()
            {
                _settingStore = new Dictionary<StoresSupportedType, object>();
            }

            public static void Register(StoresSupportedType type, ISettingStore store)
            {
                if (!_settingStore.ContainsKey(type))
                {
                    _settingStore.Add(type, store);
                }
                else
                {
                    _settingStore[type] = store;
                }
            }

            internal static object Get(StoresSupportedType type)
            {
                if (!_settingStore.ContainsKey(type))
                {
                    return null;
                }

                return _settingStore[type];
            }
        }
    }
}
