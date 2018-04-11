using System.Globalization;

namespace AppCfg.TypeParsers
{
    internal class DecimalParser : ITypeParser<decimal>
    {
        public decimal Parse(string rawValue, ITypeParserOptions optionsl)
        {
            return decimal.Parse(rawValue, NumberStyles.Any, TypeParserSettings.DefaultCulture);
        }
    }
}
