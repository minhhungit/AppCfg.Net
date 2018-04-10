using Newtonsoft.Json;

namespace AppCfg
{
    public interface IJsonDataType
    {
        JsonSerializerSettings BuildJsonSerializerSettings();
    }
}
