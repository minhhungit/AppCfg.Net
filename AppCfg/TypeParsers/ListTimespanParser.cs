using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCfg.TypeParsers
{
    public class ListTimespanParser : ITypeParser<List<TimeSpan>>
    {
        public List<TimeSpan> Parse(string rawValue, ITypeParserOptions options)
        {
            var separator = options.Separator ?? ";";

            var parser = new TimeSpanParser();
            return new List<TimeSpan>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => parser.Parse(s, options)));
        }
    }

    public class IReadOnlyListTimespanParser : ITypeParser<IReadOnlyList<TimeSpan>>
    {
        public IReadOnlyList<TimeSpan> Parse(string rawValue, ITypeParserOptions options)
        {
            return new ListTimespanParser().Parse(rawValue, options);
        }
    }
}
