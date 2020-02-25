using AppCfg;
using AppCfg.SettingStore;
using System.Configuration;

namespace AppCfgDemo
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
            // by default, AppCfg will auto create [JsonParser] for json type (IJsonDataType) at runtime
            // so, if you want to overwrite it by your parser [DemoParserWithRawBuilder] then you have to register it
            MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonPerson>());
            MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonHelloWorld>());

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
