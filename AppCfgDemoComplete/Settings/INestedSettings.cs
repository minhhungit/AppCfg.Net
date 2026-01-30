using AppCfg;
using System;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates nested settings where one interface contains another.
    /// </summary>
    public interface INestedSettings
    {
        [Option(Alias = "Nested:DatabaseName")]
        string DatabaseName { get; }

        // Nested interface property
        IConnectionStringSettings ConnectionStrings { get; }
    }

    public interface INestedInnerSettings
    {
        [Option(Alias = "Nested:CacheTimeout")]
        TimeSpan CacheTimeout { get; }
    }
}
