using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AppCfg.TypeParsers
{
    public class ListIntParser : ITypeParser<List<int>>
    {
        public List<int> Parse(string rawValue, ITypeParserOptions options)
        {
            var separator = options.Separator ?? ";";

            var parser = new IntParser();
            return new List<int>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => parser.Parse(s, options)));
        }
    }

    public class IReadOnlyListIntParser : ITypeParser<IReadOnlyList<int>>
    {
        public IReadOnlyList<int> Parse(string rawValue, ITypeParserOptions options)
        {
            return new ListIntParser().Parse(rawValue, options);
        }
    }
}
