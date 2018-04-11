using System.Globalization;

namespace AppCfg.TypeParsers
{
    internal class DecimalParser : ITypeParser<decimal>
    {
        public decimal Parse(string rawValue)
        {
            return decimal.Parse(rawValue, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        }
    }
}
