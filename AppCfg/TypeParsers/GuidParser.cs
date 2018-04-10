using System;

namespace AppCfg.TypeParsers
{
    internal class GuidParser : ITypeParser<Guid>
    {
        public Guid Parse(string rawValue)
        {
            return Guid.Parse(rawValue);
        }
    }
}
