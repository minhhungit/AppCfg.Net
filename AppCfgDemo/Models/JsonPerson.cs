using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AppCfgDemo
{
    public class JsonPerson : IJsonDataType
    {
        public string Title { get; }
        public PersonProperty Properties { get; }
        public List<string> Required { get; }

        [JsonProperty(PropertyName = "test-title")]
        public string TestTitle { get; }
    }

    public class PersonProperty
    {
        public AgeProperty Age { get; }
    }

    public class AgeProperty
    {
        public string Description { get; }
        public string Type { get; }
        public int Minimum { get; }
    }
}
