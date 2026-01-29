using System;

namespace AppCfg
{
    /// <summary>
    /// Specifies default StoreType and StoreIdentity for all properties in an interface.
    /// Individual properties can override these defaults using [Option] attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class DefaultOptionAttribute : Attribute
    {
        /// <summary>
        /// Default store type for all properties in this interface
        /// </summary>
        public SettingStoreType StoreType { get; set; }

        /// <summary>
        /// Default store identity for all properties in this interface
        /// </summary>
        public string StoreIdentity { get; set; }

        public DefaultOptionAttribute()
        {
            StoreType = SettingStoreType.AppSetting;
        }
    }
}
