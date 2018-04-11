using System;
using System.Globalization;

namespace AppCfg.TypeParsers
{
    internal class DateTimeParser : ITypeParser<DateTime>
    {
        public DateTime Parse(string rawValue)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings

            // Short Date Pattern ("d"): MM/dd/yyyy
            // Long Date Pattern ("D"): dddd, dd MMMM yyyy
            // Full Date Short Time ("f"): dddd, dd MMMM yyyy HH:mm
            // Full Date Long Time ("F"): dddd, dd MMMM yyyy HH:mm:ss
            // General Date Short Time ("g"): MM/dd/yyyy HH:mm
            // General Date Long Time ("G"): MM/dd/yyyy HH:mm:ss
            // Month ("M", "m"): MMMM dd
            // Round-Trip ("O", "o"): yyyy-MM-ddTHH:mm:ss.fffffffK
            // RFC1123 ("R", "r"): ddd, dd MMM yyyy HH:mm:ss GMT
            // Sortable ("s"): yyyy-MM-ddTHH:mm:ss
            // Short Time ("t"): HH:mm
            // Long Time ("T"): HH:mm:ss
            // Universal Sortable ("u"): yyyy-MM-dd HH:mm:ssZ
            // Universal Full ("U"): dddd, dd MMMM yyyy HH:mm:ss
            // Year Month ("Y", "y"): yyyy MMMM

            return DateTime.Parse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
    }
}
