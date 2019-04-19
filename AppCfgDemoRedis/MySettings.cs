using AppCfg;
using AppCfg.SettingStore;

namespace AppCfgDemoRedis
{
    /// <summary>
    /// Setting wrapper class, this also help us cache setting
    /// In case you want to refresh setting value every time you get setting (for example if you get setting from database)
    /// then you have to call MyAppCfg.Get<Your_Setting_Here>() directly
    /// </summary>
    public class MySettings
    {

        public static void Init()
        {
            MyAppCfg.SettingStores.RegisterRedisDatabaseStore("my_test", new RedisSettingStoreConfig
            {                
                GetRawValueFunc = (storeIdentity, tenantKey, settingKey) =>
                {
                    var redisKeyPrefix = "AppCfgNet_";

                    // get redis value at here
                    return $"Here is value for key <{redisKeyPrefix}{storeIdentity}_{tenantKey}_{settingKey}>: {System.DateTime.Now.Ticks}";
                }
            });

            // inital settings
            BaseSettings = MyAppCfg.Get<ISetting>();
        }

        public static ISetting BaseSettings;
    }
}