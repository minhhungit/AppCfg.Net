using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace AppCfg.TypeParsers
{
    public class JsonTypeParser<T> : ITypeParser<T> where T: class, IJsonDataType, new()
    {
        public T Parse(string rawValue)
        {
            var jsonSettings = new JsonSerializerSettings();

            var iObj = Activator.CreateInstance<T>();
            if (iObj is IJsonDataType jsonDataType)
            {
                var js = jsonDataType.BuildJsonSerializerSettings();
                if (js != null)
                {
                    jsonSettings = js;
                }                
            }

            jsonSettings.ContractResolver = new PrivateSetterContractResolver();

            return JsonConvert.DeserializeObject<T>(rawValue, jsonSettings);
        }
    }
}
