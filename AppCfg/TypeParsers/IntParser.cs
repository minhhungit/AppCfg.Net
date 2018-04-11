using System.Globalization;

namespace AppCfg.TypeParsers
{
    internal class IntParser : ITypeParser<int>
    {
        public int Parse(string rawValue, string inputFormat = null, string separator = null)
        {
            return int.Parse(rawValue, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        }
    }
}
