using AppCfg;
using Newtonsoft.Json;
using System;

namespace AppCfgDemo
{
    public class Animal : IJsonDataType
    {
        public string Name { get; }
        public int Legs { get; }
        public bool CanSwim { get;  }
        public DateTime SampleDay { get; }

        public JsonSerializerSettings BuildJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                DateFormatString = "dd+MM+yyyy"
            };
        }
    }
}
