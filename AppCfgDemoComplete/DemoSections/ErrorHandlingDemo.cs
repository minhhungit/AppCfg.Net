using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class ErrorHandlingDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Error Handling Demo",
                "Demonstrates how AppCfg handles configuration errors:\n" +
                "  - Missing values\n" +
                "  - Type conversion errors\n" +
                "  - Default value fallbacks"
            );

            try
            {
                MySettings.InitializeBasic();

                OutputHelper.WriteHeader("Valid Values");
                TryGetSetting("ValidInt", () =>
                {
                    var settings = MyAppCfg.Get<IErrorSettings>();
                    return settings.ValidInt.ToString();
                });

                TryGetSetting("ValidGuid", () =>
                {
                    var settings = MyAppCfg.Get<IErrorSettings>();
                    return settings.ValidGuid.ToString();
                });

                OutputHelper.WriteHeader("Error Cases");

                TryGetSetting("InvalidInt (type error)", () =>
                {
                    var settings = MyAppCfg.Get<IErrorSettings>();
                    return settings.InvalidInt.ToString();
                });

                TryGetSetting("MissingValue", () =>
                {
                    var settings = MyAppCfg.Get<IErrorSettings>();
                    return settings.MissingValue ?? "null";
                });

                TryGetSetting("MissingWithDefault", () =>
                {
                    var settings = MyAppCfg.Get<IErrorSettings>();
                    return settings.MissingWithDefault;
                });

                TryGetSetting("InvalidGuid (type error)", () =>
                {
                    var settings = MyAppCfg.Get<IErrorSettings>();
                    return settings.InvalidGuid.ToString();
                });

                Console.WriteLine();
                OutputHelper.WriteInfo("Error handling insights:");
                Console.WriteLine("  ✓ Type conversion errors throw clear exceptions");
                Console.WriteLine("  ✓ Missing values return null (reference types) or throw (value types)");
                Console.WriteLine("  ✓ DefaultValue attribute provides fallbacks");
                Console.WriteLine("  ✓ Errors occur at property access, not at Get<T>() call");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Unexpected error: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }

        private static void TryGetSetting(string name, Func<string> getter)
        {
            try
            {
                var value = getter();
                OutputHelper.WriteSuccess($"{name}: {value}");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"{name}: {ex.GetType().Name}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"    {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
