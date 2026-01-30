using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class AdvancedFeaturesDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Advanced Features Demo",
                "Demonstrates advanced AppCfg features:\n" +
                "  - IReadOnlyList<T> for immutable collections\n" +
                "  - RawValue attribute for inline default values"
            );

            try
            {
                MySettings.InitializeBasic();
                var settings = MyAppCfg.Get<IAdvancedSettings>();

                OutputHelper.WriteHeader("IReadOnlyList Collections");
                Console.Write("  ReadOnlyInts: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{string.Join(", ", settings.ReadOnlyInts)}]");
                Console.ResetColor();
                Console.WriteLine($"    Type: {settings.ReadOnlyInts.GetType().Name}");

                Console.Write("  ReadOnlyStrings: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{string.Join(", ", settings.ReadOnlyStrings)}]");
                Console.ResetColor();
                Console.WriteLine($"    Type: {settings.ReadOnlyStrings.GetType().Name}");

                OutputHelper.WriteHeader("RawValue Attribute (Inline Defaults)");
                Console.Write("  NumbersWithInlineDefault: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{string.Join(", ", settings.NumbersWithInlineDefault)}]");
                Console.ResetColor();
                Console.WriteLine("  Note: This uses RawValue inline default since key doesn't exist in config");

                Console.WriteLine();
                Console.Write("  RegularString: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"'{settings.RegularString}'");
                Console.ResetColor();
                Console.WriteLine("  Note: Regular strings are automatically trimmed");

                Console.WriteLine();
                OutputHelper.WriteSuccess("Advanced features working correctly!");
                Console.WriteLine();
                Console.WriteLine("Benefits:");
                Console.WriteLine("  ✓ IReadOnlyList enforces immutability");
                Console.WriteLine("  ✓ RawValue provides inline fallback values");
                Console.WriteLine("  ✓ Better type safety and control");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run AdvancedFeatures demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
