using System;

namespace AppCfg.TypeParsers
{
    internal class TimeSpanParser : ITypeParser<TimeSpan>
    {
        public TimeSpan Parse(string rawValue, ITypeParserOptions options)
        {
            if (options.InputFormat == null)
            {
                return TimeSpan.Parse(rawValue, TypeParserSettings.DefaultCulture);
            }
            else
            {
                return TimeSpan.ParseExact(rawValue, options.InputFormat, TypeParserSettings.DefaultCulture);
            }
        }
    }
}
