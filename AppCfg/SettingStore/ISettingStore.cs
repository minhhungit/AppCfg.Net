namespace AppCfg.SettingStore
{
    public interface ISettingStore
    {
        SettingStoreType SettingStoreType { get; }
        string StoreIdentity { get; }
    }
}
