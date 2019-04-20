using AppCfg;

namespace AppCfgDemoRedis
{
    public interface IRedisSetting
    {
        [Option(Alias = "Author")]
        [StoreOption(SettingStoreType.Custom)]
        string ASettingFromDb_Text { get; }

        [Option(Alias = "PartnerKey")]
        [StoreOption(SettingStoreType.Custom, MySettings.StoreKey_Redis)]
        string ASettingFromDb_Stored { get; }
    }

    /// <summary>
    /// Setting wrapper class, this also help us cache setting
    /// In case you want to refresh setting value every time you get setting (for example if you get setting from database)
    /// then you have to call MyAppCfg.Get<Your_Setting_Here>() directly
    /// </summary>
    public class MySettings
    {
        public const string StoreKey_Redis = "Redis > demo";

        public static void Init()
        {
            // without store identity (default)
            MyAppCfg.SettingStores.RegisterCustomStore(opt =>
                   {
                       var redisKeyPrefix = "AppCfgNet_";

                       // get redis value at here
                       return $"[NULL StoreKey] Here is value for key <{redisKeyPrefix}{opt.StoreIdentity}_{opt.TenantKey}_{opt.SettingKey}>: {System.DateTime.Now.Ticks}";
                   });

            MyAppCfg.SettingStores.RegisterCustomStore(StoreKey_Redis, opt =>
                {
                    var redisKeyPrefix = "AppCfgNet_";

                    // get redis value at here
                    return $"[WITH StoreKey] Here is value for key <{redisKeyPrefix}{opt.StoreIdentity}_{opt.TenantKey}_{opt.SettingKey}>: {System.DateTime.Now.Ticks}";
                });
        }
    }
}