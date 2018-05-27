namespace AppCfg.TypeParsers
{
    internal class BooleanParser : ITypeParser<bool>
    {
        public bool Parse(string rawValue, ITypeParserOptions options = null)
        {
            rawValue = rawValue.Trim().ToLower();

            switch (rawValue)
            {
                case "1":
                case "true":
                case "yes":
                case "y":
                    return true;
                case "0":
                case "false":
                case "no":
                case "n":
                    return false;
                default:
                    return bool.Parse(rawValue);
            }
        }
    }
}
