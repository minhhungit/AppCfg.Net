using System;

namespace AppCfg.TypeParsers
{
    internal class GuidParser : ITypeParser<Guid>
    {
        public Guid Parse(string rawValue, string inputFormat = null, string separator = null)
        {
            if (inputFormat == null)
            {
                return Guid.Parse(rawValue);
            }
            else
            {
                return Guid.ParseExact(rawValue, inputFormat);
            }
        }
    }
}
