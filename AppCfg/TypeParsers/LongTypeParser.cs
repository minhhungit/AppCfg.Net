namespace AppCfg.TypeParsers
{
    internal class LongTypeParser : ITypeParser<long>
    {
        public long Parse(string rawValue)
        {
            return long.Parse(rawValue);
        }
    }
}
