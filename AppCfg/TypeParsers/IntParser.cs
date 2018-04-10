namespace AppCfg.TypeParsers
{
    internal class IntParser : ITypeParser<int>
    {
        public int Parse(string rawValue)
        {
            return int.Parse(rawValue);
        }
    }
}
