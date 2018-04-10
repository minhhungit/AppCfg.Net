using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace AppCfg
{
    public class MyAppCfg
    {
        private static Dictionary<Type, object> _settingItemStores = new Dictionary<Type, object>();

        public static T Get<T>()
        {
            if (_settingItemStores.ContainsKey(typeof(T)))
            {
                return (T)_settingItemStores[typeof(T)];
            }

            T setting = new SettingTypeMixer<object>().ExtendWith<T>();

            var props = setting.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (TypeParserFactory.Stores.ContainsKey(prop.PropertyType) && TypeParserFactory.Stores[prop.PropertyType] != null)
                {
                    var optAttr = prop.GetCustomAttribute<OptionAttribute>();
                    
                    var value = GetRawValue(optAttr?.Alias ?? prop.Name);
                    if (value != null)
                    {
                        prop.SetValue(setting, typeof(ITypeParser<>).MakeGenericType(prop.PropertyType).GetMethod("Parse").Invoke(TypeParserFactory.Stores[prop.PropertyType], new[] { value }), null);
                    }
                }
                else
                {
                    throw new Exception($"There is no type parser for type [{prop.PropertyType}]. You maybe need to create a custom type parser for it");
                }
            }

            if (!_settingItemStores.ContainsKey(typeof(T)))
            {
                _settingItemStores.Add(typeof(T), setting);
            }

            return setting;
        }

        private static string GetRawValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
