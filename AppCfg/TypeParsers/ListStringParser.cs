using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCfg.TypeParsers
{
    public class ListStringParser : ITypeParser<List<string>>
    {
        public List<string> Parse(string rawValue, ITypeParserOptions options)
        {
            var separator = options.Separator ?? ";";
            return rawValue.Split(new string[] { separator }, StringSplitOptions.None).ToList();
        }
    }

    public class IReadOnlyListStringParser : ITypeParser<IReadOnlyList<string>>
    {
        public IReadOnlyList<string> Parse(string rawValue, ITypeParserOptions options)
        {
            return new ListStringParser().Parse(rawValue, options);
        }
    }
}
