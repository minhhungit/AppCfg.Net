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
        public const string StoreKey_One = "first-key";
        public const string StoreKey_Two = "second-key";

        public static void Init()
        {
            // by default, AppCfg will auto create [JsonParser] for json type (IJsonDataType) at runtime
            // so, if you want to overwrite it by your parser [DemoParserWithRawBuilder] then you have to register it
            MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonPerson>());

            // we can also get setting from database. 
            // use StoreOption attribute to direct it
            MyAppCfg.SettingStores.RegisterMsSqlDatabaseStore(StoreKey_One,
                new MsSqlSettingStoreConfig
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["myConnSecond"].ConnectionString,
                    QueryCmd = "SELECT TOP 1 [Value] FROM [GlobalSettings] WHERE [Name] = '{0}' AND [SettingGroup] = 'product'",
                    QueryCmdType = QueryCmdType.Text,
                });

            MyAppCfg.SettingStores.RegisterMsSqlDatabaseStore(StoreKey_Two,
                new MsSqlSettingStoreConfig
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["myConnSecond"].ConnectionString,
                    QueryCmd = "AppCfgGetSetting",
                    QueryCmdType = QueryCmdType.StoreProcedure,
                });

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
