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
}
