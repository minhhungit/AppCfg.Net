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
            return new List<int>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => int.Parse(s, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture)));
        }
    }
}
