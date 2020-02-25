using AppCfg.SettingStore;
using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        public static JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Load settings
        /// </summary>
        /// <typeparam name="TSetting"></typeparam>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public static TSetting Get<TSetting>()
        {
            return Get<TSetting>(null);
        }

        /// <summary>
        /// Load settings for a specific tenant
        /// </summary>
        /// <typeparam name="TSetting"></typeparam>
        /// <param name="tenantKey"></param>
        /// <returns></returns>
        public static TSetting Get<TSetting>(string tenantKey)
        {
            TSetting setting = new AppCfgTypeMixer<object>().ExtendWith<TSetting>();

            var props = setting.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (TypeParsers.Get(prop.PropertyType) == null)
                {
                    object settingObj = null;                   

                    if (prop.PropertyType.IsInterface)
                    {
                        var myMethod = typeof(MyAppCfg)
                             .GetMethods()
                             .Where(m => m.Name == "Get")
                             .Select(m => new
                             {
                                 Method = m,
                                 Params = m.GetParameters(),
                                 Args = m.GetGenericArguments()
                             })
                             .Where(x => x.Params.Length == 1 && x.Params[0].Name == "tenantKey")
                             .Select(x => x.Method)
                             .First();

                        MethodInfo genericMethod = myMethod.MakeGenericMethod(prop.PropertyType);
                        settingObj = genericMethod.Invoke(null, new[] { tenantKey });
                        prop.SetValue(setting, settingObj);
                        continue;
                    }
                    else
                    {
                        settingObj = Activator.CreateInstance(prop.PropertyType);

                        if (settingObj is IJsonDataType) // auto register json parser for types which inherited from IJsonDataType
                        {
                            var jsParser = Activator.CreateInstance(typeof(JsonParser<>).MakeGenericType(prop.PropertyType));
                            TypeParsers.Register(prop.PropertyType, jsParser);
                        }

                        if (settingObj is Enum) // auto register json parser for types which inherited from IJsonDataType
                        {
                            var t = typeof(EnumParser<>).MakeGenericType(prop.PropertyType);
                            object eumParser = Activator.CreateInstance(t);

                            TypeParsers.Register(prop.PropertyType, eumParser);
                        }
                    }
                }

                if(TypeParsers.Get(prop.PropertyType) != null)
                {
                    try
                    {
                        ITypeParserOptions parserOpt = (ITypeParserOptions)prop.GetCustomAttribute<OptionAttribute>() ?? new OptionAttribute();

                        string rawValue = null;

                        if (parserOpt?.RawValue != null)
                        {
                            rawValue = parserOpt.RawValue;
                        }
                        else
                        {
                            var settingKey = parserOpt?.Alias ?? prop.Name;
                            if (typeof(ITypeParserRawBuilder<>).MakeGenericType(prop.PropertyType).IsAssignableFrom(TypeParsers.Get(prop.PropertyType).GetType()))
                            {
                                rawValue = (string)typeof(ITypeParserRawBuilder<>).MakeGenericType(prop.PropertyType).GetMethod("GetRawValue").Invoke(TypeParsers.Get(prop.PropertyType), new[] { settingKey });
                            }
                            else
                            {
                                rawValue = GetRawValue(prop.PropertyType, tenantKey, settingKey, parserOpt);
                            }
                        }                        
                        
                        if (rawValue != null)
                        {
                            prop.SetValue(setting, typeof(ITypeParser<>).MakeGenericType(prop.PropertyType).GetMethod("Parse").Invoke(TypeParsers.Get(prop.PropertyType), new[] { rawValue, (object)parserOpt }), null);
                        }
                        else
                        {
                            prop.SetValue(setting, parserOpt?.DefaultValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        var tParserType = TypeParsers.Get(prop.PropertyType) != null ? TypeParsers.Get(prop.PropertyType).GetType().ToString() : "null";
                        throw new AppCfgException($"{ex.InnerException?.Message ?? ex.Message}\n - Setting: {typeof(TSetting)}\n - Property Name: {prop.Name}\n - Property Type: {prop.PropertyType}\n - Parser: {tParserType}", ex);
                    }
                }
                else
                {
                    throw new AppCfgException($"There is no type parser for type [{prop.PropertyType}]. You maybe need to create a custom type parser for it");
                }                
            }

            return setting;
        }
    }
}
