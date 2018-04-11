namespace AppCfg.TypeParsers
{
    internal class StringParser : ITypeParser<string>
    {
        public string Parse(string rawValue, string inputFormat = null, string separator = null)
        {
            return rawValue;
        }
    }
}
