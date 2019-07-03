namespace AppCfg
{
    public enum SettingStoreType
    {
        AppSetting,
        Custom
    }

    public interface ITypeParserOptions
    {
        string Alias { get; }
        object DefaultValue { get; }
        string RawValue { get; }
        string InputFormat { get; }
        string Separator { get; }

        SettingStoreType StoreType { get; }
        string StoreIdentity { get; }
    }

    public interface ITypeParser<T>
    {
        T Parse(string rawValue, ITypeParserOptions options);
    }

    public interface ITypeParserRawBuilder<T> : ITypeParser<T>
    {
        string GetRawValue(string settingKey);
    }
}
