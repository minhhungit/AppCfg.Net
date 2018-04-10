using AppCfg.TypeParsers;
using System;
using System.Collections.Generic;

namespace AppCfg
{
    public class TypeParserFactory
    {
        internal static Dictionary<Type, object> Stores { get; private set; }

        static TypeParserFactory()
        {
            Stores = new Dictionary<Type, object>();

            AddParser(new IntParser());
            AddParser(new LongParser());
            AddParser(new GuidParser());
        }

        public static void AddParser<T>(ITypeParser<T> item)
        {
            if (!Stores.ContainsKey(typeof(T)))
            {
                Stores.Add(typeof(T), item);
            }
            else
            {
                throw new Exception("Duplicate type parser");
            }
        }

        public static void RemoveParser(Type type)
        {
            if (Stores.ContainsKey(type))
            {
                Stores.Remove(type);
            }
        }
    }
}
