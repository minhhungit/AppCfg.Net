using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System;

namespace AppCfgDemoComplete.Models
{
    public class Machine : IJsonDataTypeWithSetting
    {
        public DateTime DayWithNewFormat { get; }

        public JsonSerializerSettings BuildJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                DateFormatString = "dd|MMM+yyyy"
            };
        }
    }
}
