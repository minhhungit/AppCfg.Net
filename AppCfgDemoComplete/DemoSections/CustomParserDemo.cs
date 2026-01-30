using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class CustomParserDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Custom Parser Demo - REGISTRATION API",
                "Demonstrates ITypeParserRawBuilder for custom type parsing.\n" +
                "This example loads JSON from external files instead of inline strings.\n" +
                "Shows how to REGISTER custom parsers using MyAppCfg.TypeParsers.Register()."
            );

            try
            {
                MySettings.InitializeBasic();
                var settings = MyAppCfg.Get<ICustomParserSettings>();

                OutputHelper.WriteHeader("JsonPerson (from external file)");
                OutputHelper.WriteSetting("Title", settings.DemoRawBuilder.Title, "JSON file");
                OutputHelper.WriteSetting("TestTitle", settings.DemoRawBuilder.TestTitle, "JSON file");
                OutputHelper.WriteSetting("Age.Type", settings.DemoRawBuilder.Properties.Age.Type, "JSON file");
                OutputHelper.WriteSetting("Age.Minimum", settings.DemoRawBuilder.Properties.Age.Minimum, "JSON file");

                OutputHelper.WriteHeader("JsonHelloWorld (from external file)");
                OutputHelper.WriteSetting("Name", settings.HelloWorld.Name, "JSON file");
                OutputHelper.WriteSetting("Age", settings.HelloWorld.MyAge, "JSON file");

                Console.WriteLine();
                OutputHelper.WriteSuccess("Custom parser working correctly!");
                Console.WriteLine();
                Console.WriteLine("Custom parser registration in MySettings.cs:");
                Console.WriteLine("  MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonPerson>());");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run CustomParser demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
