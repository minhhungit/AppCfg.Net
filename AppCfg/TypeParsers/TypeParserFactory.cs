using System;
using System.Collections.Generic;

namespace AppCfg.TypeParsers
{
    public class TypeParserFactory
    {
        public TypeParserFactory()
        {
            TypeParserStores = new Dictionary<Type, object>();

            AddParser(new IntTypeParser());
            AddParser(new LongTypeParser());
            AddParser(new GuidTypeParser());
        }

        internal static Dictionary<Type, object> TypeParserStores { get; private set; }

        public void AddParser<T>(ITypeParser<T> item)
        {
            if (!TypeParserStores.ContainsKey(typeof(T)))
            {
                TypeParserStores.Add(typeof(T), item);
            }
            else
            {
                throw new Exception("Duplicate type parser");
            }
        }

        public void RemoveParser<T>(ITypeParser<T> item)
        {
            if (TypeParserStores.ContainsKey(typeof(T)))
            {
                TypeParserStores.Remove(typeof(T));
            }
        }
    }
}
