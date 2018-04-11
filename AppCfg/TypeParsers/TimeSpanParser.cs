using System;

namespace AppCfg.TypeParsers
{
    internal class TimeSpanParser : ITypeParser<TimeSpan>
    {
        public TimeSpan Parse(string rawValue, string inputFormat = null, string separator = null)
        {
            if (inputFormat == null)
            {
                return TimeSpan.Parse(rawValue, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                return TimeSpan.ParseExact(rawValue, inputFormat, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}
