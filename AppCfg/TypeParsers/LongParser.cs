namespace AppCfg.TypeParsers
{
    internal class LongParser : ITypeParser<long>
    {
        public long Parse(string rawValue, string inputFormat = null, string separator = null)
        {
            return long.Parse(rawValue, System.Globalization.NumberStyles.Integer | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
