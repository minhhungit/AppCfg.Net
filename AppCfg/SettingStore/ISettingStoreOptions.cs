namespace AppCfg.SettingStore
{
    public interface ISettingStoreOptions
    {
        SettingStoreType SettingStoreType { get; }
        string StoreIdentity { get; }
    }
}
