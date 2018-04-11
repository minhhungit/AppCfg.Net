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
            First = MyAppCfg.Get<ISetting>();
            Second = MyAppCfg.Get<IJsonSetting>();
        }

        public static ISetting First;
        public static IJsonSetting Second;
    }
}
