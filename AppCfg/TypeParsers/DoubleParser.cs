using System.Globalization;

namespace AppCfg.TypeParsers
{
    internal class DoubleParser : ITypeParser<double>
    {
        public double Parse(string rawValue, ITypeParserOptions options)
        {
            return double.Parse(rawValue, NumberStyles.Any, TypeParserSettings.DefaultCulture);
        }
    }
}
