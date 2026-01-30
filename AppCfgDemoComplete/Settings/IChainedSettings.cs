using AppCfg;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates priority-based configuration loading.
    /// After calling MyAppCfg.Configure(), all settings automatically use:
    /// Priority order: Environment Variables → User Secrets → App.config
    /// This is the RECOMMENDED approach for production applications.
    /// </summary>
    public interface IChainedSettings
    {
        [Option(Alias = "ChainedDemo:AppConfigValue")]
        string AppConfigValue { get; }

        [Option(Alias = "ChainedDemo:SecretValue")]
        string SecretValue { get; }

        [Option(Alias = "ChainedDemo:EnvVarValue")]
        string EnvVarValue { get; }

        [Option(Alias = "ChainedDemo:OverrideMe")]
        string OverrideMe { get; }
    }
}
