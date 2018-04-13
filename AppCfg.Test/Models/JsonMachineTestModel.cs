using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System;

namespace AppCfg.Test
{
    public class JsonMachineTestModel : IJsonDataTypeWithSetting
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
