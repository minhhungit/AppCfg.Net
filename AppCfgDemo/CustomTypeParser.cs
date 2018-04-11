using AppCfg;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AppCfgDemo
{
    #region List<int>
    public class ListIntParser : ITypeParser<List<int>>
    {
        public List<int> Parse(string rawValue, string inputFormat = null, string separator = null)
        {
            separator = separator ?? ";";
            return new List<int>(rawValue.Split(new string[] { separator }, StringSplitOptions.None).Select(s => int.Parse(s, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture)));
        }
    }
    #endregion

    #region DateTimeShort
    public class DateTimeLongType
    {
        public DateTimeLongType(DateTime value)
        {
            Value = value;
        }

        public DateTime Value { get; }
    }

    public class DateTimeLongTypeParser : ITypeParser<DateTimeLongType>
    {
        public DateTimeLongType Parse(string rawValue, string inputFormat = null, string separator = null)
        {
            return new DateTimeLongType(DateTime.ParseExact(rawValue, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
    #endregion

}
