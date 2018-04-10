using AppCfg.ConfigurationStores;
using AppCfg.TypeParsers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace AppCfg
{
    public class MyAppCfg
    {
        private static ConfigurationStoreType _configurationStoreType = ConfigurationStoreType.AppSetting;

        public static TypeParserFactory TypeParserFactory = new TypeParserFactory();

        //public MyAppCfg(string configurationStore)
        //{
        //    _configurationStore = configurationStore;            
        //}

        static Dictionary<Type, object> _stores = new Dictionary<Type, object>();
        public static T Get<T>()
        {
            if (_stores.ContainsKey(typeof(T)))
            {
                return (T)_stores[typeof(T)];
            }

            T setting = SettingTypeMixer<object>.ExtendWith<T>();

            var props = setting.GetType().GetProperties();

            foreach (var prop in props)
            {
                if (TypeParserFactory.TypeParserStores.ContainsKey(prop.PropertyType) && TypeParserFactory.TypeParserStores[prop.PropertyType] != null)
                {
                    var optAttr = prop.GetCustomAttribute<OptionAttribute>();

                    var value = GetRawValue(_configurationStoreType, optAttr?.Alias ?? prop.Name);
                    if (value != null)
                    {
                        prop.SetValue(setting, typeof(ITypeParser<>).MakeGenericType(prop.PropertyType).GetMethod("Parse").Invoke(TypeParserFactory.TypeParserStores[prop.PropertyType], new[] { value }), null);
                    }
                }
                else
                {
                    throw new Exception($"There is no type parser for type [{prop.PropertyType}]. You maybe need to create a custom type parser for it");
                }
            }

            if (!_stores.ContainsKey(typeof(T)))
            {
                _stores.Add(typeof(T), setting);
            }

            return setting;
        }

        private static object GetRawValue(ConfigurationStoreType cfgStoreType, string keyName)
        {
            if(cfgStoreType == ConfigurationStoreType.AppSetting)
            {
                return ConfigurationManager.AppSettings[keyName];
            }
            else
            {
                return null; // @todo: need to implement others config stores
            }
        }
    }
}
