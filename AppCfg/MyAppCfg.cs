using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        public static JsonSerializerSettings JsonSerializerSettings { get; set; }

        public static TSetting Get<TSetting>()
        {
            if (TypeParsers.Get(typeof(TSetting)) != null)
            {
                return (TSetting)TypeParsers.Get(typeof(TSetting));
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
                                rawValue = GetRawValue(prop.PropertyType, settingKey, iOptions);
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

            if (TypeParsers.Get(typeof(TSetting)) == null)
            {
                TypeParsers.Register(typeof(TSetting), setting);
            }

            return setting;
        }
    }
}
