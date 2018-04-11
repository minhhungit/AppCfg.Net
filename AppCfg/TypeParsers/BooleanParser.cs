namespace AppCfg.TypeParsers
{
    internal class BooleanParser : ITypeParser<bool>
    {
        public bool Parse(string rawValue, string inputFormat = null, string separator = null)
        {
            rawValue = rawValue.Trim().ToLower();

            switch (rawValue)
            {
                case "1":
                case "true":
                case "yes":
                    return true;
                case "0":
                case "false":
                case "no":
                    return false;
                default:
                    return bool.Parse(rawValue);
            }
        }
    }
}
