using System.Globalization;

namespace AppCfg.TypeParsers
{
    internal class IntParser : ITypeParser<int>
    {
        public int Parse(string rawValue, ITypeParserOptions options)
        {
            return int.Parse(rawValue, NumberStyles.Any, TypeParserSettings.DefaultCulture);
        }
    }
}
