namespace AppCfg
{
    public enum SettingStoreType
    {
        AppConfig,
        MsSqlDatabase
    }

    public interface ITypeParserOptions
    {
        string Alias { get; }
        object DefaultValue { get; }
        string RawValue { get; }
        string InputFormat { get; }
        string Separator { get; }
        SettingStoreType SettingStoreType { get; }
    }

    internal class DefaultTypeParserOption : ITypeParserOptions
    {
        public DefaultTypeParserOption()
        {
            SettingStoreType = SettingStoreType.AppConfig;
        }

        public string Alias { get; }
        public object DefaultValue { get; }
        public string RawValue { get; }
        public string InputFormat { get; }
        public string Separator { get; }
        public SettingStoreType SettingStoreType { get; }
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
