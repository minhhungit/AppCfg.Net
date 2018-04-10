using System;

namespace AppCfg.TypeParsers
{
    internal class TimeSpanParser : ITypeParser<TimeSpan>
    {
        public TimeSpan Parse(string rawValue)
        {
            return TimeSpan.Parse(rawValue, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
