namespace AppCfg.TypeParsers
{
    internal class IntTypeParser : ITypeParser<int>
    {
        public int Parse(string rawValue)
        {
            return int.Parse(rawValue);
        }
    }
}
