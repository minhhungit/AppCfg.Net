using System;
using System.Configuration;
using System.Data.SqlClient;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        private static string GetRawValue(Type typeOfSetting, string tenantKey, string settingNameKey, ITypeParserOptions parserOpt)
        {
            var profileKey = parserOpt.ProfileKey;

            // If ProfileKey is null or empty, check if Configure() was called
            if (string.IsNullOrEmpty(profileKey))
            {
                // If Configure() was called, use priority-based loading (the default chain)
                if (_isConfigured && _defaultStoreChain != null)
                {
                    return _defaultStoreChain(settingNameKey, typeOfSetting);
                }

                // Otherwise, use traditional App.config/Web.config
                return GetRawValueForAppSettingStore(typeOfSetting, settingNameKey);
            }

            // For explicit profile keys, look up the registered store
            return GetRawValueForProfileStore(profileKey, tenantKey, typeOfSetting, settingNameKey);
        }

        private static string GetRawValueForAppSettingStore(Type typeOfSetting, string settingNameKey)
        {
            if (typeOfSetting == typeof(SqlConnectionStringBuilder))
            {
                return ConfigurationManager.ConnectionStrings[settingNameKey.ToString()]?.ConnectionString;
            }
            else
            {
                return ConfigurationManager.AppSettings[settingNameKey.ToString()];
            }
        }

        private static string GetRawValueForProfileStore(string profileKey, string tenantKey, Type typeOfSetting, string settingKey)
        {
            if (SettingStores.Get(profileKey) is Func<SettingStoreMetadata, string> getRawValueFunc)
            {
                if (getRawValueFunc != null)
                {
                    return getRawValueFunc.Invoke(new SettingStoreMetadata(profileKey, tenantKey, settingKey, typeOfSetting));
                }
                else
                {
                    throw new Exception($"GetRawValueFunc is null for profile key '{profileKey}'");
                }
            }
            else
            {
                throw new AppCfgException($"No store registered for profile key '{profileKey}'. Please call MyAppCfg.SettingStores.RegisterStore(\"{profileKey}\", ...) first.");
            }
        }
    }
}
