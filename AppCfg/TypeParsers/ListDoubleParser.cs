using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCfg.TypeParsers
{
    public class ListDoubleParser : ITypeParser<List<double>>
    {
        public List<double> Parse(string rawValue, ITypeParserOptions options)
        {
            var separator = options.Separator ?? ";";

            var parser = new DoubleParser();
            return new List<double>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => parser.Parse(s, options)));
        }
    }

    public class IReadOnlyListDoubleParser : ITypeParser<IReadOnlyList<double>>
    {
        public IReadOnlyList<double> Parse(string rawValue, ITypeParserOptions options)
        {
            return new ListDoubleParser().Parse(rawValue, options);
        }
    }
}
