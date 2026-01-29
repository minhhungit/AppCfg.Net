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
        /// <summary>
        /// Default store identity used by MyAppCfg.Configure()
        /// Use this in [DefaultOption] attribute to use the configured stores
        /// </summary>
        public const string DefaultStoreIdentity = "AppCfg:Default";

        public static JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Configure AppCfg with automatic priority-based configuration loading.
        /// This is the recommended way to set up AppCfg.Net
        ///
        /// Priority order:
        /// 1. Environment Variables (highest priority) - perfect for Docker/Kubernetes
        /// 2. User Secrets (if provided) - for local development
        /// 3. AppSettings (app.config/web.config) - fallback defaults
        /// </summary>
        /// <param name="envVarPrefix">Environment variable prefix (default: "APPCFG__"). Use double underscore for hierarchy.</param>
        /// <param name="userSecretsId">User secrets directory name (optional). If null, user secrets are skipped.</param>
        /// <example>
        /// <code>
        /// // Configure in your startup:
        /// MyAppCfg.Configure(
        ///     envVarPrefix: "APPCFG__",
        ///     userSecretsId: "my-app-secrets"
        /// );
        ///
        /// // Use in your interface:
        /// [DefaultOption(StoreType = SettingStoreType.Custom,
        ///                StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
        /// public interface IAppSettings
        /// {
        ///     [Option(Alias = "ApiKey")]
        ///     string ApiKey { get; }
        /// }
        ///
        /// // Load settings:
        /// var settings = MyAppCfg.Get&lt;IAppSettings&gt;();
        /// </code>
        /// </example>
        public static void Configure(string envVarPrefix = "APPCFG__", string userSecretsId = null)
        {
            ChainedStore.Register(DefaultStoreIdentity, envVarPrefix, userSecretsId);
        }

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

            // Get interface-level defaults if they exist
            var interfaceDefaults = typeof(TSetting).GetCustomAttribute<DefaultOptionAttribute>();

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

                        if (settingObj is Enum)
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
                        var propOption = prop.GetCustomAttribute<OptionAttribute>() ?? new OptionAttribute();

                        // Merge property-level options with interface-level defaults
                        var parserOpt = MergeOptionWithDefaults(propOption, interfaceDefaults);
                        ITypeParserOptions parserOptInterface = parserOpt;

                        string rawValue = null;

                        if (parserOptInterface?.RawValue != null)
                        {
                            rawValue = parserOptInterface.RawValue;
                        }
                        else
                        {
                            var settingKey = parserOptInterface?.Alias ?? prop.Name;
                            if (typeof(ITypeParserRawBuilder<>).MakeGenericType(prop.PropertyType).IsAssignableFrom(TypeParsers.Get(prop.PropertyType).GetType()))
                            {
                                rawValue = (string)typeof(ITypeParserRawBuilder<>).MakeGenericType(prop.PropertyType).GetMethod("GetRawValue").Invoke(TypeParsers.Get(prop.PropertyType), new[] { settingKey });
                            }
                            else
                            {
                                rawValue = GetRawValue(prop.PropertyType, tenantKey, settingKey, parserOptInterface);
                            }
                        }

                        if (rawValue != null)
                        {
                            prop.SetValue(setting, typeof(ITypeParser<>).MakeGenericType(prop.PropertyType).GetMethod("Parse").Invoke(TypeParsers.Get(prop.PropertyType), new[] { rawValue, (object)parserOptInterface }), null);
                        }
                        else
                        {
                            prop.SetValue(setting, parserOptInterface?.DefaultValue);
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

        /// <summary>
        /// Merges property-level [Option] attributes with interface-level [DefaultOption] attributes.
        ///
        /// Rules:
        /// 1. If property sets StoreIdentity (even to empty string), it's explicitly configuring the store
        /// 2. Otherwise, inherit from interface defaults
        ///
        /// To override back to AppSetting from Custom default: Set StoreIdentity = "" (empty string)
        /// </summary>
        private static OptionAttribute MergeOptionWithDefaults(OptionAttribute propOption, DefaultOptionAttribute interfaceDefaults)
        {
            if (interfaceDefaults == null)
            {
                // No interface defaults, return property options as-is
                return propOption;
            }

            // Create a new OptionAttribute with merged values
            var merged = new OptionAttribute
            {
                Alias = propOption.Alias,
                DefaultValue = propOption.DefaultValue,
                RawValue = propOption.RawValue,
                InputFormat = propOption.InputFormat,
                Separator = propOption.Separator
            };

            // Check if property explicitly set StoreIdentity (including empty string)
            // We consider null as "not set", any other value (including "") as "set"
            var propertySetStoreIdentity = propOption.StoreIdentity != null;

            if (propertySetStoreIdentity)
            {
                // Property explicitly configured store (even if empty for AppSetting override)
                merged.StoreType = propOption.StoreType;
                merged.StoreIdentity = propOption.StoreIdentity;
            }
            else
            {
                // Property didn't set StoreIdentity, inherit interface defaults
                merged.StoreType = interfaceDefaults.StoreType;
                merged.StoreIdentity = interfaceDefaults.StoreIdentity;
            }

            return merged;
        }
    }
}
