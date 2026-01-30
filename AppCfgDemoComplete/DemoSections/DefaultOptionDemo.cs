using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class DefaultOptionDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "DefaultOption Attribute Demo",
                "Demonstrates [DefaultOption] attribute for setting a default ProfileKey.\n" +
                "When used with custom stores, reduces repetition by applying ProfileKey to all properties.\n" +
                "Note: This demo shows basic usage without custom stores."
            );

            try
            {
                MySettings.InitializeBasic();
                var settings = MyAppCfg.Get<IDefaultOptionSettings>();

                OutputHelper.WriteHeader("Settings Values");
                OutputHelper.WriteSetting("Category", settings.Category, "App.config");
                OutputHelper.WriteSetting("Priority", settings.Priority, "App.config");

                Console.WriteLine();
                OutputHelper.WriteInfo("DefaultOption is most useful with custom stores:");
                Console.WriteLine();
                Console.WriteLine("Example with database store:");
                Console.WriteLine("  [DefaultOption(ProfileKey = \"MyDatabaseStore\")]");
                Console.WriteLine("  public interface ISettings {");
                Console.WriteLine("    string Setting1 { get; }  // Uses MyDatabaseStore");
                Console.WriteLine("    string Setting2 { get; }  // Uses MyDatabaseStore");
                Console.WriteLine("  }");
                Console.WriteLine();
                Console.WriteLine("Without DefaultOption:");
                Console.WriteLine("  public interface ISettings {");
                Console.WriteLine("    [Option(ProfileKey = \"MyDatabaseStore\")]");
                Console.WriteLine("    string Setting1 { get; }");
                Console.WriteLine("    [Option(ProfileKey = \"MyDatabaseStore\")]");
                Console.WriteLine("    string Setting2 { get; }");
                Console.WriteLine("  }");
                Console.WriteLine();
                OutputHelper.WriteSuccess("DefaultOption reduces repetitive ProfileKey declarations!");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run DefaultOption demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
