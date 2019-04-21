using System;

namespace AppCfgDemoMssql
{
    class Program
    {
        static void Main(string[] args)
        {
            MySettings.Init();

            var settingsWithoutTenant = MySettings.BaseSettings;
            var settingsWithTenant = MySettings.BaseSettingsWithTenant;

            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.ASettingFromDb_Text}");
            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.DemoDefault_Text}");
            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.ASettingFromDb_Stored}");
            Console.WriteLine();

            Console.WriteLine($"With Tenant: {settingsWithTenant.ASettingFromDb_Text}");
            Console.WriteLine($"With Tenant: {settingsWithTenant.DemoDefault_Text}");
            Console.WriteLine($"With Tenant: {settingsWithTenant.ASettingFromDb_Stored}");

            Console.ReadKey();
        }
    }
}
