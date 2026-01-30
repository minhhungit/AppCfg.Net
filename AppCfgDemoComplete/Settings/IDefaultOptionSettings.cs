using AppCfg;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates DefaultOption attribute which applies a default ProfileKey to all properties.
    /// Properties can still use individual Alias to map to specific config keys.
    /// </summary>
    public interface IDefaultOptionSettings
    {
        [Option(Alias = "DefaultDemo:Category")]
        string Category { get; }

        [Option(Alias = "DefaultDemo:Priority")]
        string Priority { get; }
    }
}
