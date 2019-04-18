using AppCfg;
using System;

namespace AppCfgDemoMssql
{
    public interface ISetting
    {
        [Option(Alias = "Author")]
        [StoreOption(SettingStoreType.MsSqlDatabase, MySettings.StoreKey_One)]
        string ASettingFromDb_Text { get; }

        [Option(Alias = "PartnerKey")]
        [StoreOption(SettingStoreType.MsSqlDatabase, MySettings.StoreKey_Two)]
        Guid ASettingFromDb_Stored { get; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // NOTE: 
            // 1. You need to create a database and tables, check 'test_database.sql' script first, please !!!
            // 2. Update [myConn] connectionstring in App.config

            MySettings.Init();

            var settingsWithoutTenant = MyAppCfg.Get<ISetting>();
            var settingsWithTenant = MyAppCfg.Get<ISetting>("I am a tenant");

            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.ASettingFromDb_Text}");
            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.ASettingFromDb_Stored}");
            Console.WriteLine();

            Console.WriteLine($"With Tenant: {settingsWithTenant.ASettingFromDb_Text}");
            Console.WriteLine($"With Tenant: {settingsWithTenant.ASettingFromDb_Stored}");

            Console.ReadKey();
        }
    }
}
