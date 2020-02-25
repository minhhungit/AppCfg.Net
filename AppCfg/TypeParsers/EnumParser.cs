using System;

namespace AppCfg.TypeParsers
{
    public class EnumParser<T> : ITypeParser<T> where T: struct
    {
        public T Parse(string rawValue, ITypeParserOptions options)
        {
            var isDigit = IsDigit(rawValue.Trim());

            if (isDigit)
            {
                return (T)Enum.ToObject(typeof(T), int.Parse(rawValue));
            }
            else
            {
                if (Enum.TryParse(rawValue, out T result))
                {
                    return result;
                }            
            }

            throw new AppCfgException($"Can not convert [{rawValue}] to enum");
        }

        private bool IsDigit(string value)
        {
            foreach (var c in value)
            {
                if (!Char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
