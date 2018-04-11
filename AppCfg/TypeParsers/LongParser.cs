namespace AppCfg.TypeParsers
{
    internal class LongParser : ITypeParser<long>
    {
        public long Parse(string rawValue)
        {
            return long.Parse(rawValue, System.Globalization.NumberStyles.Integer | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
