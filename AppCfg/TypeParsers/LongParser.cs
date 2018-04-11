using System.Globalization;

namespace AppCfg.TypeParsers
{
    internal class LongParser : ITypeParser<long>
    {
        public long Parse(string rawValue, ITypeParserOptions options)
        {
            return long.Parse(rawValue, NumberStyles.Any, TypeParserSettings.DefaultCulture);
        }
    }
}
