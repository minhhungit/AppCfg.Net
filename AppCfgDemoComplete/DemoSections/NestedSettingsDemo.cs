using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class NestedSettingsDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Nested Settings Demo",
                "Demonstrates nested interface configuration.\n" +
                "One interface can contain properties of another interface type.\n" +
                "Great for organizing complex configuration hierarchies."
            );

            try
            {
                MySettings.InitializeBasic();
                var settings = MyAppCfg.Get<INestedSettings>();

                OutputHelper.WriteHeader("Top-Level Properties");
                OutputHelper.WriteSetting("DatabaseName", settings.DatabaseName, "App.config");

                OutputHelper.WriteHeader("Nested ConnectionStrings Interface");
                OutputHelper.WriteSetting("First.DataSource", settings.ConnectionStrings.First.DataSource, "connectionStrings");
                OutputHelper.WriteSetting("First.InitialCatalog", settings.ConnectionStrings.First.InitialCatalog, "connectionStrings");
                Console.WriteLine();
                OutputHelper.WriteSetting("Second.DataSource", settings.ConnectionStrings.Second.DataSource, "connectionStrings");
                OutputHelper.WriteSetting("Second.InitialCatalog", settings.ConnectionStrings.Second.InitialCatalog, "connectionStrings");

                Console.WriteLine();
                OutputHelper.WriteSuccess("Nested interfaces work seamlessly!");
                Console.WriteLine();
                Console.WriteLine("Interface structure:");
                Console.WriteLine("  INestedSettings");
                Console.WriteLine("    └─ IConnectionStringSettings ConnectionStrings");
                Console.WriteLine("         ├─ SqlConnectionStringBuilder First");
                Console.WriteLine("         └─ SqlConnectionStringBuilder Second");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run NestedSettings demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
