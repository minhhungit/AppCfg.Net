using Newtonsoft.Json;

namespace AppCfg.TypeParsers
{
    public interface IJsonDataType
    {
        
    }

    public interface IJsonDataTypeWithSetting : IJsonDataType
    {
        JsonSerializerSettings BuildJsonSerializerSettings();
    }

    public class JsonParser<T> : ITypeParser<T> where T: class, IJsonDataType, new()
    {
        public T Parse(string rawValue, ITypeParserOptions options)
        {
            var jsonSettings = new JsonSerializerSettings();
            if (MyAppCfg.JsonSerializerSettings != null)
            {
                jsonSettings = MyAppCfg.JsonSerializerSettings;
            }            

            var iObj = System.Activator.CreateInstance<T>();
            if (iObj is IJsonDataTypeWithSetting jsonDataType)
            {
                var js = jsonDataType.BuildJsonSerializerSettings();
                if (js != null)
                {
                    jsonSettings = js;
                }
            }

            if (jsonSettings.ContractResolver == null)
            {
                jsonSettings.ContractResolver = new PrivateSetterContractResolver();
            }            

            return JsonConvert.DeserializeObject<T>(rawValue, jsonSettings);
        }
    }

    
}
