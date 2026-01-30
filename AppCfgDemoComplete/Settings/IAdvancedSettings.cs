using AppCfg;
using System.Collections.Generic;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates advanced features: IReadOnlyList for immutable collections and RawValue for inline defaults.
    /// </summary>
    public interface IAdvancedSettings
    {
        // IReadOnlyList - immutable collection
        [Option(Alias = "Advanced:ReadOnlyInts", Separator = ";")]
        IReadOnlyList<int> ReadOnlyInts { get; }

        [Option(Alias = "Advanced:ReadOnlyStrings", Separator = ",")]
        IReadOnlyList<string> ReadOnlyStrings { get; }

        // RawValue - provides an inline default value (useful when config might not exist)
        [Option(Alias = "Advanced:MissingValue", RawValue = "10;20;30", Separator = ";")]
        List<int> NumbersWithInlineDefault { get; }

        // Regular string to show trimming behavior
        [Option(Alias = "Advanced:RegularString")]
        string RegularString { get; }
    }
}
