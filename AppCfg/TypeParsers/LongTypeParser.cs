namespace AppCfg.TypeParsers
{
    public class LongTypeParser : ITypeParser<long>
    {
        public long Parse(string rawValue)
        {
            return long.Parse(rawValue);
        }
    }
}
