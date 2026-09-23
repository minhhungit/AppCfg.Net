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

        // Internal flag to track if Configure() was called
        private static bool _isConfigured = false;
        private static Func<string, Type, string> _defaultStoreChain = null;

        /// <summary>
        /// Configure AppCfg with automatic priority-based configuration loading.
        /// This is the recommended way to set up AppCfg.Net
        ///
        /// Priority order:
        /// 1. Environment Variables (highest priority) - perfect for Docker/Kubernetes
        /// 2. User Secrets (if provided) - for local development
        /// 3. AppSettings (app.config/web.config) - fallback defaults
        ///
        /// After calling Configure(), all settings without explicit ProfileKey will automatically
        /// use this priority-based loading. No need for [DefaultOption] attributes!
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
        /// // Just define your interface:
        /// public interface IAppSettings
        /// {
        ///     [Option(Alias = "ApiKey")]
        ///     string ApiKey { get; }
        /// }
        ///
        /// // Load settings - automatically uses priority-based loading!
        /// var settings = MyAppCfg.Get&lt;IAppSettings&gt;();
        /// </code>
        /// </example>
        public static void Configure(string envVarPrefix = "APPCFG__", string userSecretsId = null)
        {
            _defaultStoreChain = ChainedStore.BuildChain(envVarPrefix, userSecretsId);
            _isConfigured = true;
        }

        /// <summary>
        /// Undo <see cref="Configure(string, string)"/>. Settings without an explicit ProfileKey go back to
        /// reading App.config/Web.config only. Mainly useful in tests.
        /// </summary>
        public static void ResetConfiguration()
        {
            _defaultStoreChain = null;
            _isConfigured = false;
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

            // Track computed properties to process after config properties are loaded
            var computedProperties = new System.Collections.Generic.List<PropertyInfo>();

            foreach (var prop in props)
            {
                // Check if this is a computed property
                var computedAttr = prop.GetCustomAttribute<ComputedAttribute>();
                if (computedAttr != null)
                {
                    computedProperties.Add(prop);
                    continue; // Skip loading from config
                }
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

            // Process computed properties after all config properties are loaded
            foreach (var computedProp in computedProperties)
            {
                try
                {
                    var computedAttr = computedProp.GetCustomAttribute<ComputedAttribute>();

                    // Find the static method
                    var method = computedAttr.HelperType.GetMethod(
                        computedAttr.MethodName,
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new[] { typeof(TSetting) },
                        null
                    );

                    if (method == null)
                    {
                        throw new AppCfgException(
                            $"Computed method not found: {computedAttr.HelperType.FullName}.{computedAttr.MethodName}(${typeof(TSetting).Name}). " +
                            $"Ensure the method is public, static, and accepts a single parameter of type {typeof(TSetting).Name}."
                        );
                    }

                    // Verify return type matches property type
                    if (method.ReturnType != computedProp.PropertyType)
                    {
                        throw new AppCfgException(
                            $"Computed method return type mismatch for property '{computedProp.Name}'. " +
                            $"Expected: {computedProp.PropertyType.Name}, but method returns: {method.ReturnType.Name}"
                        );
                    }

                    // Invoke the method with the settings instance
                    var computedValue = method.Invoke(null, new object[] { setting });

                    // Set the property value
                    computedProp.SetValue(setting, computedValue);
                }
                catch (Exception ex)
                {
                    throw new AppCfgException(
                        $"Error computing property '{computedProp.Name}': {ex.InnerException?.Message ?? ex.Message}\n" +
                        $" - Setting: {typeof(TSetting)}\n" +
                        $" - Property Type: {computedProp.PropertyType}",
                        ex
                    );
                }
            }

            return setting;
        }

        /// <summary>
        /// Merges property-level [Option] attributes with interface-level [DefaultOption] attributes.
        ///
        /// Rules:
        /// 1. If property sets ProfileKey (even to empty string), it's explicitly configuring the store
        /// 2. Otherwise, inherit from interface defaults
        /// 3. If no ProfileKey is set anywhere (null), settings will be loaded from App.config
        /// </summary>
        private static OptionAttribute MergeOptionWithDefaults(OptionAttribute propOption, DefaultOptionAttribute interfaceDefaults)
        {
            // Create a new OptionAttribute with merged values
            var merged = new OptionAttribute
            {
                Alias = propOption.Alias,
                DefaultValue = propOption.DefaultValue,
                RawValue = propOption.RawValue,
                InputFormat = propOption.InputFormat,
                Separator = propOption.Separator
            };

            // Check if property explicitly set ProfileKey (including empty string)
            // We consider null as "not set", any other value (including "") as "set"
            var propertySetProfileKey = propOption.ProfileKey != null;

            if (propertySetProfileKey)
            {
                // Property explicitly configured profile key
                merged.ProfileKey = propOption.ProfileKey;
            }
            else if (interfaceDefaults != null && interfaceDefaults.ProfileKey != null)
            {
                // Property didn't set ProfileKey, inherit interface defaults
                merged.ProfileKey = interfaceDefaults.ProfileKey;
            }
            else
            {
                // No ProfileKey set anywhere, use null (will load from App.config)
                merged.ProfileKey = null;
            }

            return merged;
        }
    }
}
