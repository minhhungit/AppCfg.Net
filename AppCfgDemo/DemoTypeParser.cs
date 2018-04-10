using AppCfg;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace AppCfgDemo
{
    public class Person
    {
        public string Name { get; }
        public DateTime Birthday { get; }
        public int Age { get; }
        public bool IsMarried { get;  }
    }

    public class PersonParser : ITypeParser<Person>
    {
        public Person Parse(string rawValue)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateSetterContractResolver(),
                DateFormatString = "MM-dd/yyyy"
            };

            return JsonConvert.DeserializeObject<Person>(rawValue, settings);
        }
    }
}
