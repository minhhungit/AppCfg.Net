namespace AppCfg.SettingStore
{
    public class RedisSettingStoreConfig
    {
        public System.Func<string, string, string, string> GetRawValueFunc { get; set; }
    }
}
