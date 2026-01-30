using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class BasicTypesDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Basic Types Demo",
                "Demonstrates all built-in primitive types and list collections.\n" +
                "AppCfg.Net supports 22+ built-in type parsers out of the box."
            );

            try
            {
                MySettings.InitializeBasic();
                var settings = MyAppCfg.Get<IBasicSettings>();

                OutputHelper.WriteHeader("Primitive Types");
                OutputHelper.WriteSetting("Boolean", settings.DemoBoolean, "App.config");
                OutputHelper.WriteSetting("DateTime", settings.DemoDateTime, "App.config");
                OutputHelper.WriteSetting("DateTime (Custom Format)", settings.DemoDateTimeWithFormat.ToString("MMM dd, yyyy"), "App.config");
                OutputHelper.WriteSetting("Decimal", settings.DemoDecimal, "App.config");
                OutputHelper.WriteSetting("Double", settings.DemoDouble, "App.config");
                OutputHelper.WriteSetting("Guid", settings.DemoGuid, "App.config");
                OutputHelper.WriteSetting("Int", settings.DemoInt, "App.config");
                OutputHelper.WriteSetting("Long", settings.DemoLong, "App.config");
                OutputHelper.WriteSetting("String", settings.DemoString, "App.config");
                OutputHelper.WriteSetting("TimeSpan (HH:MM:SS)", settings.DemoTimeSpanFirst, "App.config");
                OutputHelper.WriteSetting("TimeSpan (DD:HH:MM:SS)", settings.DemoTimeSpanSecond, "App.config");

                OutputHelper.WriteHeader("Enum Types");
                OutputHelper.WriteSetting("Enum by Int", settings.EnumByInt, "App.config");
                OutputHelper.WriteSetting("Enum by String", settings.EnumByString, "App.config");
                OutputHelper.WriteSetting("Enum Another", settings.HelloEnum, "App.config");

                OutputHelper.WriteHeader("List Collections");

                Console.Write("  Numbers (separator: ^): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{string.Join(", ", settings.Numbers)}]");
                Console.ResetColor();

                Console.Write("  Strings (separator: ;): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{string.Join(", ", settings.StringList)}]");
                Console.ResetColor();

                Console.Write("  Guids (separator: |): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{settings.GuidList.Count} items");
                Console.ResetColor();

                Console.Write("  Booleans (separator: ,): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{string.Join(", ", settings.BoolList)}]");
                Console.ResetColor();

                Console.Write("  DateTimes (separator: ;): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{settings.DateTimeList.Count} items");
                Console.ResetColor();

                Console.WriteLine();
                OutputHelper.WriteSuccess("All type parsers working correctly!");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run BasicTypes demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
