using System;

namespace AppCfg.TypeParsers
{
    internal class GuidTypeParser : ITypeParser<Guid>
    {
        public Guid Parse(string rawValue)
        {
            return Guid.Parse(rawValue);
        }
    }
}
