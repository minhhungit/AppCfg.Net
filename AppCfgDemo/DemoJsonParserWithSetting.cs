using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System;

namespace AppCfgDemo
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
