namespace AppCfg.TypeParsers
{
    public class IntTypeParser : ITypeParser<int>
    {
        public int Parse(string rawValue)
        {
            return int.Parse(rawValue);
        }
    }
}
