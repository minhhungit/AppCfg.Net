using System;

namespace AppCfg.TypeParsers
{
    internal class GuidParser : ITypeParser<Guid>
    {
        public Guid Parse(string rawValue, ITypeParserOptions options)
        {
            if (options.InputFormat == null)
            {
                return Guid.Parse(rawValue);
            }
            else
            {
                return Guid.ParseExact(rawValue, options.InputFormat);
            }
        }
    }
}
