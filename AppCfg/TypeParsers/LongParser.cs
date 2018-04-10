namespace AppCfg.TypeParsers
{
    internal class LongParser : ITypeParser<long>
    {
        public long Parse(string rawValue)
        {
            return long.Parse(rawValue, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
