using AppCfg;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates reading configuration from environment variables.
    /// Set environment variables with prefix: APPCFG__
    /// Example: APPCFG__Env__SimpleString = "from-environment"
    /// </summary>
    public interface IEnvironmentSettings
    {
        [Option(Alias = "Env:SimpleString")]
        string SimpleString { get; }

        [Option(Alias = "Env:IntValue")]
        int IntValue { get; }
    }
}
