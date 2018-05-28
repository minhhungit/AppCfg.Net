using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCfg.TypeParsers
{
    public class ListLongParser : ITypeParser<List<long>>
    {
        public List<long> Parse(string rawValue, ITypeParserOptions options)
        {
            var separator = options.Separator ?? ";";

            var parser = new LongParser();
            return new List<long>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => parser.Parse(s, options)));
        }
    }

    public class IReadOnlyListLongParser : ITypeParser<IReadOnlyList<long>>
    {
        public IReadOnlyList<long> Parse(string rawValue, ITypeParserOptions options)
        {
            return new ListLongParser().Parse(rawValue, options);
        }
    }
}
