using AppCfg;
using AppCfg.SettingStore;
using System;

namespace AppCfgDemoEnvironmentVariables
{
    public class MySettings
    {
        public static IEnvironmentSettings EnvironmentSettings { get; set; }
        public static IMixedSettings MixedSettings { get; set; }

        public static void Init()
        {
            // Register the Environment Variables store with default prefix "APPCFG__"
            EnvironmentVariableStore.Register();

            // Load settings
            EnvironmentSettings = MyAppCfg.Get<IEnvironmentSettings>();
            MixedSettings = MyAppCfg.Get<IMixedSettings>();
        }
    }

    /// <summary>
    /// Example using DefaultOption attribute to avoid repetition.
    /// All properties will use Environment Variables store unless overridden.
    /// </summary>
    [DefaultOption(StoreType = SettingStoreType.Custom,
                   StoreIdentity = "EnvironmentVariables:APPCFG__")]
    public interface IEnvironmentSettings
    {
        [Option(Alias = "ApiKey", DefaultValue = "")]
        string ApiKey { get; }

        [Option(Alias = "Database:Host", DefaultValue = "localhost")]
        string DatabaseHost { get; }

        [Option(Alias = "Database:Port", DefaultValue = 5432)]
        int DatabasePort { get; }

        [Option(Alias = "Database:Password", DefaultValue = "")]
        string DatabasePassword { get; }

        [Option(Alias = "Feature:Enabled", DefaultValue = false)]
        bool FeatureEnabled { get; }

        [Option(Alias = "Feature:MaxRetries", DefaultValue = 3)]
        int MaxRetries { get; }

        [Option(Alias = "ClientId")]
        Guid ClientId { get; }
    }

    /// <summary>
    /// Example mixing environment variables with AppSettings.
    /// Shows how DefaultOption sets the default but individual properties can override.
    /// </summary>
    [DefaultOption(StoreType = SettingStoreType.Custom,
                   StoreIdentity = "EnvironmentVariables:APPCFG__")]
    public interface IMixedSettings
    {
        // Uses environment variable (from DefaultOption)
        [Option(Alias = "SecretKey", DefaultValue = "")]
        string SecretKey { get; }

        // Overrides to use AppSettings instead
        // Note: StoreIdentity must be set (even to empty string) to override
        [Option(Alias = "PublicSetting",
                StoreType = SettingStoreType.AppSetting,
                StoreIdentity = "",
                DefaultValue = "default-public")]
        string PublicSetting { get; }
    }
}
