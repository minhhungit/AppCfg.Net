using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCfg.TypeParsers
{
    public class ListDecimalParser : ITypeParser<List<decimal>>
    {
        public List<decimal> Parse(string rawValue, ITypeParserOptions options)
        {
            var separator = options.Separator ?? ";";

            var parser = new DecimalParser();
            return new List<decimal>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => parser.Parse(s, options)));
        }
    }

    public class IReadOnlyListDecimalParser : ITypeParser<IReadOnlyList<decimal>>
    {
        public IReadOnlyList<decimal> Parse(string rawValue, ITypeParserOptions options)
        {
            return new ListDecimalParser().Parse(rawValue, options);
        }
    }
}
