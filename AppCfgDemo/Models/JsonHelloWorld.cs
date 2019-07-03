using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AppCfgDemo
{
    public class JsonHelloWorld : IJsonDataType
    {
        public string Name { get; }

        [JsonProperty(PropertyName = "age")]
        public int MyAge { get; }
    }
}
