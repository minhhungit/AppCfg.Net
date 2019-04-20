using AppCfg.SettingStore;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        private static string GetRawValue(Type typeOfSetting, string tenantKey, string settingNameKey, ITypeParserOptions parserOpt, ISettingStore storeOpt)
        {
            switch (storeOpt.SettingStoreType)
            {
                case SettingStoreType.AppSetting:
                    return GetRawValueForAppSettingStore(typeOfSetting, settingNameKey);
                case SettingStoreType.Custom:
                    return GetRawValueForCustomStore(storeOpt.SettingStoreType, storeOpt.StoreIdentity, tenantKey, typeOfSetting, settingNameKey);;
            }

            throw new Exception($"Settting store {storeOpt.SettingStoreType} is not supported");
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

        private static string GetRawValueForCustomStore(SettingStoreType settingStoreType, string storeIdentity, string tenantKey, Type typeOfSetting, string settingKey)
        {
            if (SettingStores.Get(SettingStoreType.Custom, storeIdentity) is Func<SettingStoreMetadata, string> getRawValueFunc)
            {
                if (getRawValueFunc != null)
                {
                    return getRawValueFunc.Invoke(new SettingStoreMetadata(storeIdentity, tenantKey, settingKey, typeOfSetting));
                }
                else
                {
                    throw new Exception("Please setup method 'GetRawValueFunc(storeIdentity, tenantKey, settingKey)' for Custom source");
                }
            }
            else
            {
                throw new AppCfgException($"Please config for Custom source: Can not load sourceConfig for storeIdentity {storeIdentity}");
            }
        }        
    }
}
