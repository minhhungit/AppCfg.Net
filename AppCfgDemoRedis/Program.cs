using AppCfg;
using System;

namespace AppCfgDemoRedis
{
    public interface ISetting
    {
        [Option(Alias = "Author")]
        [StoreOption(SettingStoreType.Redis, "my_test")]
        string ASettingFromDb_Text { get; }

        [Option(Alias = "PartnerKey")]
        [StoreOption(SettingStoreType.Redis, "my_test")]
        string ASettingFromDb_Stored { get; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            MySettings.Init();

            var settingsWithoutTenant = MyAppCfg.Get<ISetting>();
            var settingsWithTenant = MyAppCfg.Get<ISetting>("demo-tenant");

            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.ASettingFromDb_Text}");
            Console.WriteLine($"Without Tenant: {settingsWithoutTenant.ASettingFromDb_Stored}");
            Console.WriteLine();

            Console.WriteLine($"With Tenant: {settingsWithTenant.ASettingFromDb_Text}");
            Console.WriteLine($"With Tenant: {settingsWithTenant.ASettingFromDb_Stored}");

            Console.ReadKey();
        }
    }
}
