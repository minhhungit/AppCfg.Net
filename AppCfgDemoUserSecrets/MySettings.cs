using AppCfg;
using AppCfg.SettingStore;
using System;

namespace AppCfgDemoUserSecrets
{
    public class MySettings
    {
        public const string UserSecretsId = "appcfg-demo-secrets";

        public static ISecretSettings SecretSettings { get; set; }

        public static void Init()
        {
            // Configure AppCfg - ONE LINE!
            // Automatic priority: Env Vars → User Secrets → AppSettings
            MyAppCfg.Configure(
                envVarPrefix: "APPCFG__",
                userSecretsId: UserSecretsId
            );

            // Load settings - automatically checks all sources!
            SecretSettings = MyAppCfg.Get<ISecretSettings>();
        }
    }

    /// <summary>
    /// Settings interface with automatic priority-based loading:
    /// 1. Environment Variables (checked first)
    /// 2. User Secrets
    /// 3. AppSettings (fallback)
    /// No need to specify store on each property - it's automatic!
    /// </summary>
    [DefaultOption(StoreType = SettingStoreType.Custom,
                   StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
    public interface ISecretSettings
    {
        [Option(Alias = "ApiKey", DefaultValue = "")]
        string ApiKey { get; }

        [Option(Alias = "Database:Password", DefaultValue = "default-password")]
        string DatabasePassword { get; }

        [Option(Alias = "Database:Port", DefaultValue = 5432)]
        int DatabasePort { get; }

        [Option(Alias = "ClientId")]
        Guid ClientId { get; }

        [Option(Alias = "Feature:MaxRetries", DefaultValue = 3)]
        int MaxRetries { get; }

        [Option(Alias = "Feature:Enabled", DefaultValue = false)]
        bool FeatureEnabled { get; }

        // This property automatically checks:
        // 1. APPCFG__DemoValue env var
        // 2. DemoValue in secrets.json
        // 3. DemoValue in app.config
        [Option(Alias = "DemoValue", DefaultValue = "default-demo-value")]
        string DemoValue { get; }
    }
}
