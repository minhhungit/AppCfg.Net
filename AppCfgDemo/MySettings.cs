using AppCfg;
using AppCfg.SettingStore;
using System.Configuration;

namespace AppCfgDemo
{
    /// <summary>
    /// Setting wrapper class
    /// </summary>
    public class MySettings
    {
        public static void Init()
        {
            // by default, AppCfg will auto create [JsonParser] for json type (IJsonDataType) at runtime
            // so, if you want to overwrite it by your parser [DemoParserWithRawBuilder] then you have to register it
            MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonPerson>());

            // we can also get setting from database. 
            // use config SettingStoreType within Options attribute to direct it
            //MyAppCfg.SettingStores.Register(StoresSupportedType.MsSqlDatabase,
            //    new MsSqlSettingStoreConfig
            //    {
            //        ConnectionString = ConfigurationManager.ConnectionStrings["myConnSecond"].ConnectionString,
            //        QueryCmd = "SELECT TOP 1 [Value] FROM [Settings] WHERE [Name] = '{0}'",
            //        //QueryCmd = "AppCfgGetSetting", // For case you want to use Stored Proc, check sample_get_setting_store.sql for more information
            //        QueryCmdType = QueryCmdType.Text,
            //    });

            // setup json serializer settings
            MyAppCfg.JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = "dd+MM+yyyy" // ex: setup default format
            };

            // inital settings
            BaseSettings = MyAppCfg.Get<ISetting>();
            JsonSettings = MyAppCfg.Get<IJsonSetting>();

            ConnSettings = MyAppCfg.Get<IConnectionStringSetting>();
        }

        public static IConnectionStringSetting ConnSettings;
        public static ISetting BaseSettings;
        public static IJsonSetting JsonSettings;
    }
}
