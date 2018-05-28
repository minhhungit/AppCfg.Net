using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCfg.TypeParsers
{
    public class ListGuidParser : ITypeParser<List<Guid>>
    {
        public List<Guid> Parse(string rawValue, ITypeParserOptions options)
        {
            var separator = options.Separator ?? ";";

            var parser = new GuidParser();
            return new List<Guid>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => parser.Parse(s, options)));
        }
    }

    public class IReadOnlyListGuidParser : ITypeParser<IReadOnlyList<Guid>>
    {
        public IReadOnlyList<Guid> Parse(string rawValue, ITypeParserOptions options)
        {
            return new ListGuidParser().Parse(rawValue, options);
        }
    }
}
