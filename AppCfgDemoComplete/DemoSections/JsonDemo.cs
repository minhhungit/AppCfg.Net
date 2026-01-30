using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class JsonDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "JSON Configuration Demo",
                "Demonstrates parsing complex objects from JSON strings in App.config.\n" +
                "Objects must implement IJsonDataType interface."
            );

            try
            {
                MySettings.InitializeBasic();
                var settings = MyAppCfg.Get<IJsonSettings>();

                OutputHelper.WriteHeader("Animal Object from JSON");
                OutputHelper.WriteSetting("Name", settings.CuteAnimal.Name, "App.config (inline JSON)");
                OutputHelper.WriteSetting("Legs", settings.CuteAnimal.Legs, "App.config (inline JSON)");
                OutputHelper.WriteSetting("CanSwim", settings.CuteAnimal.CanSwim, "App.config (inline JSON)");
                OutputHelper.WriteSetting("SampleDay", settings.CuteAnimal.SampleDay.ToString("MMM dd, yyyy"), "App.config (inline JSON)");

                OutputHelper.WriteHeader("Machine Object with Custom Date Format");
                OutputHelper.WriteSetting("DayWithNewFormat", settings.Optimus.DayWithNewFormat.ToString("MM-dd-yyyy"), "App.config (inline JSON)");

                Console.WriteLine();
                OutputHelper.WriteSuccess("JSON parsing successful!");
                Console.WriteLine();
                Console.WriteLine("JSON configuration is stored inline in App.config:");
                Console.WriteLine("  <add key=\"cute_animal\" value=\"{Name : 'Duck', legs : '2', ...}\" />");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run JSON demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
