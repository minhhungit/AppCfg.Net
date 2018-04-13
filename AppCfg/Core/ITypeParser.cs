namespace AppCfg
{
    public interface ITypeParserOptions
    {
        string Alias { get; }
        object DefaultValue { get; }
        string InputFormat { get; }
        string Separator { get; }
    }

    internal class DefaultTypeParserOption : ITypeParserOptions
    {
        public string Alias { get; }
        public object DefaultValue { get; }
        public string InputFormat { get; }
        public string Separator { get; }
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
