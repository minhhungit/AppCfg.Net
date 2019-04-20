using AppCfg;
using System;

namespace AppCfgDemoRedis
{
    class Program
    {
        static void Main(string[] args)
        {
            MySettings.Init();

            var settingsWithoutTenant = MyAppCfg.Get<IRedisSetting>();
            var settingsWithTenant = MyAppCfg.Get<IRedisSetting>("demo-tenant");

            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.ASettingFromDb_Text}");
            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.ASettingFromDb_Stored}");
            Console.WriteLine();

            Console.WriteLine($"With Tenant: {settingsWithTenant.ASettingFromDb_Text}");
            Console.WriteLine($"With Tenant: {settingsWithTenant.ASettingFromDb_Stored}");

            Console.ReadKey();
        }
    }
}
