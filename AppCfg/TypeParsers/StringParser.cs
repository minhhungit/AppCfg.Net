namespace AppCfg.TypeParsers
{
    internal class StringParser : ITypeParser<string>
    {
        public string Parse(string rawValue, ITypeParserOptions options)
        {
            return rawValue;
        }
    }
}
