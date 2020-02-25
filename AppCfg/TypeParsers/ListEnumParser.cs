//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace AppCfg.TypeParsers
//{
//    public class ListEnumParser<T> : ITypeParser<List<T>> where T: struct
//    {
//        public List<T> Parse(string rawValue, ITypeParserOptions options)
//        {
//            var separator = options.Separator ?? ";";
//            var parser = new EnumParser<T>();
//            return new List<T>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(e => parser.Parse(e, options)));
//        }
//    }
//}
