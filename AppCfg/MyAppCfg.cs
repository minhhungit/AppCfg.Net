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
                if (TypeParsers.Get(prop.PropertyType) == null)
                {
                    var settingObj = Activator.CreateInstance(prop.PropertyType);
                    if (settingObj is IJsonDataType) // auto register json parser for types which inherited from IJsonDataType
                    {
                        var jsParser = Activator.CreateInstance(typeof(JsonParser<>).MakeGenericType(prop.PropertyType));
                        TypeParsers.Register(prop.PropertyType, jsParser);
                    }
                }

                if(TypeParsers.Get(prop.PropertyType) != null)
                {
                    try
                    {
                        ITypeParserOptions iOptions = (ITypeParserOptions)prop.GetCustomAttribute<OptionAttribute>() ?? new DefaultTypeParserOption();
                        string rawValue = null;

                        if (iOptions?.RawValue != null)
                        {
                            rawValue = iOptions.RawValue;
                        }
                        else
                        {
                            var settingKey = iOptions?.Alias ?? prop.Name;
                            if (typeof(ITypeParserRawBuilder<>).MakeGenericType(prop.PropertyType).IsAssignableFrom(TypeParsers.Get(prop.PropertyType).GetType()))
                            {
                                rawValue = (string)typeof(ITypeParserRawBuilder<>).MakeGenericType(prop.PropertyType).GetMethod("GetRawValue").Invoke(TypeParsers.Get(prop.PropertyType), new[] { settingKey });
                            }
                            else
                            {
                                rawValue = GetRawValue(prop.PropertyType, settingKey);
                            }
                        }                        
                        
                        if (rawValue != null)
                        {
                            prop.SetValue(setting, typeof(ITypeParser<>).MakeGenericType(prop.PropertyType).GetMethod("Parse").Invoke(TypeParsers.Get(prop.PropertyType), new[] { rawValue, (object)iOptions }), null);
                        }
                        else
                        {
                            prop.SetValue(setting, iOptions?.DefaultValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        var tParserType = TypeParsers.Get(prop.PropertyType) != null ? TypeParsers.Get(prop.PropertyType).GetType().ToString() : "null";
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

        public class TypeParsers
        {
            private static readonly IDictionary<Type, object> _parserStore;

            static TypeParsers()
            {
                _parserStore = new Dictionary<Type, object>();

                // initial default parsers
                Register(new BooleanParser());
                Register(new ConnectionStringParser());
                Register(new DateTimeParser());
                Register(new DecimalParser());
                Register(new DoubleParser());
                Register(new GuidParser());
                Register(new ListIntParser());
                Register(new ListStringParser());
                Register(new IntParser());
                Register(new LongParser());
                Register(new StringParser());
                Register(new TimeSpanParser());
            }

            public static void Register<T>(ITypeParser<T> parser)
            {
                var key = typeof(T);

                if (!_parserStore.ContainsKey(key))
                {
                    _parserStore.Add(key, parser);
                }
                else
                {
                    _parserStore[key] = parser;
                }
            }

            internal static void Register(Type key, object parser)
            {
                if (!_parserStore.ContainsKey(key))
                {
                    _parserStore.Add(key, parser);
                }
                else
                {
                    _parserStore[key] = parser;
                }
            }

            internal static object Get(Type propertyType)
            {
                if (!_parserStore.ContainsKey(propertyType))
                {
                    return null;
                }

                return _parserStore[propertyType];
            }
        }
    }
}
