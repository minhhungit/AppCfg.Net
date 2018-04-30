namespace AppCfg
{
    public enum SettingStoreType
    {
        AppSetting,
        MsSqlDatabase
    }

    public interface ITypeParserOptions
    {
        string Alias { get; }
        object DefaultValue { get; }
        string RawValue { get; }
        string InputFormat { get; }
        string Separator { get; }
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
