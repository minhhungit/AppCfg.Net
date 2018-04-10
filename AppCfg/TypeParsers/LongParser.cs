namespace AppCfg.TypeParsers
{
    internal class LongParser : ITypeParser<long>
    {
        public long Parse(string rawValue)
        {
            return long.Parse(rawValue);
        }
    }
}
