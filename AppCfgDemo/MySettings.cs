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
            // register custom type parser
            MyAppCfg.TypeParserFactory.AddParser(new ListIntParser());

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
