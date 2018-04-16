using AppCfg.TypeParsers;
using Newtonsoft.Json;
using System.IO;

namespace AppCfg.Test
{
    public class ParserWithRawBuilder<T> : ITypeParserRawBuilder<T> where T : IJsonDataType
    {
        /// <summary>
        /// Parse() method will use raw value from this
        /// </summary>
        /// <param name="settingKey"></param>
        /// <returns></returns>
        public string GetRawValue(string settingKey)
        {
            // here you can help AppCfg get setting value from anywhere
            // for example, pretent we get raw value from a json file

            var dir = Path.GetDirectoryName(typeof(CustomParserTest).Assembly.Location);
            using (StreamReader r = new StreamReader(Path.Combine(dir, "app.settings.json")))
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
