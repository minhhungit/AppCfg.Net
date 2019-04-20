using AppCfg.SettingStore;
using System;

namespace AppCfg
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class StoreOptionAttribute : Attribute, ISettingStore
    {
        public StoreOptionAttribute()
        {
            SettingStoreType = SettingStoreType.AppSetting;
        }

        public StoreOptionAttribute(SettingStoreType storeType)
        {
            SettingStoreType = storeType;
        }

        public StoreOptionAttribute(SettingStoreType storeType, string storeIdentity)
        {
            SettingStoreType = storeType;
            StoreIdentity = storeIdentity;
        }

        public SettingStoreType SettingStoreType { get; set; }
        public string StoreIdentity { get; set; }
    }
}
