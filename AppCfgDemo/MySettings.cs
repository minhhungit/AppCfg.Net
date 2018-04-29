using AppCfg;

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
