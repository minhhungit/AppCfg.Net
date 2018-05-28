using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCfg.TypeParsers
{
    public class ListBooleanParser : ITypeParser<List<bool>>
    {
        public List<bool> Parse(string rawValue, ITypeParserOptions options)
        {
            var separator = options.Separator ?? ";";

            var parser = new BooleanParser();
            return new List<bool>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => parser.Parse(s, options)));
        }
    }

    public class IReadOnlyListBooleanParser : ITypeParser<IReadOnlyList<bool>>
    {
        public IReadOnlyList<bool> Parse(string rawValue, ITypeParserOptions options)
        {
            return new ListBooleanParser().Parse(rawValue, options);
        }
    }
}
