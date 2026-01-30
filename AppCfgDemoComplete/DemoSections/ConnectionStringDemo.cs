using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class ConnectionStringDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Connection String Demo",
                "Demonstrates parsing connection strings into SqlConnectionStringBuilder.\n" +
                "Provides strongly-typed access to connection string properties."
            );

            try
            {
                MySettings.InitializeBasic();
                var settings = MyAppCfg.Get<IConnectionStringSettings>();

                OutputHelper.WriteHeader("First Connection String");
                OutputHelper.WriteSetting("Data Source", settings.First.DataSource, "connectionStrings");
                OutputHelper.WriteSetting("Initial Catalog", settings.First.InitialCatalog, "connectionStrings");
                OutputHelper.WriteSetting("User ID", settings.First.UserID, "connectionStrings");
                Console.WriteLine();

                OutputHelper.WriteHeader("Second Connection String");
                OutputHelper.WriteSetting("Data Source", settings.Second.DataSource, "connectionStrings");
                OutputHelper.WriteSetting("Initial Catalog", settings.Second.InitialCatalog, "connectionStrings");
                OutputHelper.WriteSetting("Integrated Security", settings.Second.IntegratedSecurity, "connectionStrings");
                OutputHelper.WriteSetting("Connect Timeout", settings.Second.ConnectTimeout, "connectionStrings");

                Console.WriteLine();
                OutputHelper.WriteSuccess("Connection strings parsed successfully!");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run ConnectionString demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
