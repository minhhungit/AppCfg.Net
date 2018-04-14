using AppCfg;
using AppCfg.TypeParsers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace AppCfgDemo
{
    public class DemoParserWithRawBuilder<T> : ITypeParserRawBuilder<T> where T : IJsonDataType
    {
        /// <summary>
        /// Parse() method will use raw value from this
        /// </summary>
        /// <param name="settingKey"></param>
        /// <returns></returns>
        public string GetRawValue(string settingKey)
        {
            // here you can help AppCfg get setting value from anywhere
            // for example, pretend we get raw value from a json file

            using (StreamReader r = new StreamReader("json-settings.json"))
            {
                return r.ReadToEnd();
            }
        }

        public T Parse(string rawValue, ITypeParserOptions options) // rawValue is come from GetRawValue()
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateSetterContractResolver()
            };

            return JsonConvert.DeserializeObject<T>(rawValue, jsonSettings);            
        }
    }
}
