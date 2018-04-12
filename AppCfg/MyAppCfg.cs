using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;

namespace AppCfg
{
    public class MyAppCfg
    {
        private static Dictionary<Type, object> _settingItemStores = new Dictionary<Type, object>();

        public static JsonSerializerSettings JsonSerializerSettings { get; set; }

        public static TSetting Get<TSetting>()
        {
            if (_settingItemStores.ContainsKey(typeof(TSetting)))
            {
                return (TSetting)_settingItemStores[typeof(TSetting)];
            }

            TSetting setting = new AppCfgTypeMixer<object>().ExtendWith<TSetting>();

            var props = setting.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!TypeParserFactory.Stores.ContainsKey(prop.PropertyType) || TypeParserFactory.Stores[prop.PropertyType] == null)
                {
                    var jsObj = Activator.CreateInstance(prop.PropertyType);
                    if (jsObj is IJsonDataType)
                    {
                        var jsParser = Activator.CreateInstance(typeof(JsonParser<>).MakeGenericType(prop.PropertyType));
                        TypeParserFactory.Stores.Add(prop.PropertyType, jsParser);
                    }
                }

                if(TypeParserFactory.Stores.ContainsKey(prop.PropertyType) && TypeParserFactory.Stores[prop.PropertyType] != null)
                {
                    try
                    {
                        ITypeParserOptions iOptions = (ITypeParserOptions)prop.GetCustomAttribute<OptionAttribute>() ?? new DefaultTypeParserOption();

                        string rawValue = GetRawValue(prop.PropertyType, iOptions?.Alias ?? prop.Name);
                        if (rawValue != null)
                        {
                            prop.SetValue(setting, typeof(ITypeParser<>).MakeGenericType(prop.PropertyType).GetMethod("Parse").Invoke(TypeParserFactory.Stores[prop.PropertyType], new[] { rawValue, (object)iOptions }), null);
                        }
                        else
                        {
                            prop.SetValue(setting, iOptions?.DefaultValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        var tParserType = TypeParserFactory.Stores.ContainsKey(prop.PropertyType) ? TypeParserFactory.Stores[prop.PropertyType].GetType().ToString() : "null";
                        throw new AppCfgException($"{ex.InnerException?.Message ?? ex.Message}\n - Setting: {typeof(TSetting)}\n - PropName: {prop.Name}\n - PropType: {prop.PropertyType}\n - Parser: {tParserType}", ex);
                    }
                }
                else
                {
                    throw new AppCfgException($"There is no type parser for type [{prop.PropertyType}]. You maybe need to create a custom type parser for it");
                }                
            }

            if (!_settingItemStores.ContainsKey(typeof(TSetting)))
            {
                _settingItemStores.Add(typeof(TSetting), setting);
            }

            return setting;
        }

        private static string GetRawValue(Type settingType, string key)
        {
            if (settingType == typeof(SqlConnectionStringBuilder))
            {
                return ConfigurationManager.ConnectionStrings[key]?.ConnectionString;
            }
            else
            {
                return ConfigurationManager.AppSettings[key];
            }            
        }

        public class TypeParserFactory
        {
            internal static Dictionary<Type, object> Stores { get; private set; }

            static TypeParserFactory()
            {
                Stores = new Dictionary<Type, object>();

                // initial default parsers
                AddParser(new BooleanParser());
                AddParser(new ConnectionStringParser());
                AddParser(new DateTimeParser());
                AddParser(new DecimalParser());
                AddParser(new DoubleParser());
                AddParser(new GuidParser());
                AddParser(new IntParser());
                AddParser(new LongParser());
                AddParser(new StringParser());
                AddParser(new TimeSpanParser());
            }

            public static void AddParser<T>(ITypeParser<T> item)
            {
                if (!Stores.ContainsKey(typeof(T)))
                {
                    Stores.Add(typeof(T), item);
                }
                else
                {
                    throw new AppCfgException("Duplicate type parser");
                }
            }

            public static void RemoveParser(Type type)
            {
                if (Stores.ContainsKey(type))
                {
                    Stores.Remove(type);
                }
            }
        }
    }
}
