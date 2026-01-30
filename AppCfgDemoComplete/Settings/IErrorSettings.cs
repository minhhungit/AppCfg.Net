using AppCfg;
using System;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates error handling for missing values and type conversion errors.
    /// </summary>
    public interface IErrorSettings
    {
        [Option(Alias = "Error:ValidInt")]
        int ValidInt { get; }

        [Option(Alias = "Error:InvalidInt")]
        int InvalidInt { get; }

        [Option(Alias = "Error:MissingValue")]
        string MissingValue { get; }

        [Option(Alias = "Error:MissingWithDefault", DefaultValue = "I am the default")]
        string MissingWithDefault { get; }

        [Option(Alias = "Error:ValidGuid")]
        Guid ValidGuid { get; }

        [Option(Alias = "Error:InvalidGuid")]
        Guid InvalidGuid { get; }
    }
}
