using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class DatabaseStoreDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Database Store Demo - REGISTRATION API",
                "Demonstrates custom MSSQL database store for configuration.\n" +
                "Shows how to REGISTER custom stores using MyAppCfg.SettingStores.RegisterStore().\n" +
                "Two approaches: CommandText (SQL query) and StoredProcedure."
            );

            if (!MySettings.IsDatabaseEnabled())
            {
                MenuHelper.ShowFeatureDisabledMessage("Database Store", "Demo:EnableDatabase");
                Console.WriteLine("Additional setup required:");
                Console.WriteLine("  1. Create AppCfgDatabase in SQL Server");
                Console.WriteLine("  2. Run the SQL script from CustomStores/MssqlStore.cs");
                Console.WriteLine("  3. Update connection string in App.config");
                MenuHelper.PauseForUser();
                return;
            }

            try
            {
                MySettings.InitializeDatabaseStore();

                if (!MySettings.IsDatabaseInitialized())
                {
                    OutputHelper.WriteError("Database store initialization failed!");
                    OutputHelper.WriteWarning("Check your connection string and ensure SQL Server is running.");
                    MenuHelper.PauseForUser();
                    return;
                }

                OutputHelper.WriteSuccess("Database store registered successfully!");
                Console.WriteLine();

                // Get settings without tenant
                var settingsNoTenant = MyAppCfg.Get<IDatabaseSettings>();

                OutputHelper.WriteHeader("Settings Without Tenant");
                OutputHelper.WriteSetting("Author (CommandText)", settingsNoTenant.AuthorFromCommandText, "SQL CommandText");
                OutputHelper.WriteSetting("Author (Default)", settingsNoTenant.AuthorDefaultValue, "DefaultValue");
                OutputHelper.WriteSetting("PartnerKey (StoredProc)", settingsNoTenant.PartnerKeyFromStoredProc, "Stored Procedure");

                Console.WriteLine();

                // Get settings with tenant
                var settingsWithTenant = MyAppCfg.Get<IDatabaseSettings>("tenant-1");

                OutputHelper.WriteHeader("Settings With Tenant 'tenant-1'");
                OutputHelper.WriteSetting("Author (CommandText)", settingsWithTenant.AuthorFromCommandText, "SQL CommandText");
                OutputHelper.WriteSetting("Author (Default)", settingsWithTenant.AuthorDefaultValue, "DefaultValue");
                OutputHelper.WriteSetting("PartnerKey (StoredProc)", settingsWithTenant.PartnerKeyFromStoredProc, "Stored Procedure");

                Console.WriteLine();
                OutputHelper.WriteInfo("Custom store registration allows loading config from any data source!");
                Console.WriteLine();
                Console.WriteLine("Registration code in MySettings.cs:");
                Console.WriteLine("  MyAppCfg.SettingStores.RegisterStore(storeKey, opt => {{ ... }});");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Database error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Common issues:");
                Console.WriteLine("  - SQL Server not running");
                Console.WriteLine("  - Database doesn't exist");
                Console.WriteLine("  - Tables/stored procedures not created");
                Console.WriteLine("  - Connection string incorrect");
            }

            MenuHelper.PauseForUser();
        }
    }
}
