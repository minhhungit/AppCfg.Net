using System.Globalization;

namespace AppCfg.TypeParsers
{
    internal class IntParser : ITypeParser<int>
    {
        public int Parse(string rawValue)
        {
            return int.Parse(rawValue, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        }
    }
}
