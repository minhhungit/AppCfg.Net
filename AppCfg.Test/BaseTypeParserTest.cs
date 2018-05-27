using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    public interface ITypeParserItemSetting
    {
        bool DemoBoolean { get; }

        [Option(Separator = ",")]
        List<bool> DemoBooleans { get; }

        DateTime DemoDateTime { get; }

        List<DateTime> DemoDateTimes { get; }

        [Option(InputFormat = "dd+MM/yyyy")]
        DateTime DemoDateTimeWithFormat { get; }

        decimal DemoDecimal { get; }

        List<decimal> DemoDecimals { get; }

        double DemoDouble { get; }
        List<double> DemoDoubles { get; }

        Guid DemoGuid { get; }
        List<Guid> DemoGuids { get; }


        [Option(DefaultValue = 77)]
        int DemoInt { get; }

        [Option(Separator = "^")]
        List<int> Numbers { get; }

        long DemoLong { get; }
        List<long> DemoLongs { get; }

        string DemoString { get; }

        [Option(Separator = "~")]
        List<string> Strings { get; }

        TimeSpan DemoTimeSpanFirst { get; }
        TimeSpan DemoTimeSpanSecond { get; }

        [Option(RawValue = "1;2;3;4;5", Separator = ";")]
        List<int> NumbersWithInitialRawValue { get; }

        List<TimeSpan> DemoTimespans { get; }
    }

    [TestFixture]
    public class BaseTypeParserTest
    {
        ITypeParserItemSetting settings = MyAppCfg.Get<ITypeParserItemSetting>();

        [Test]
        public void TypeParserValueTest()
        {
            Assert.AreEqual(settings.DemoBoolean, true);
            Assert.AreEqual(settings.DemoDateTime, new DateTime(2017, 11, 29, 23, 39, 03));
            Assert.AreEqual(settings.DemoDateTimeWithFormat, new DateTime(2015, 09, 24));
            Assert.AreEqual(settings.DemoDecimal, -12336.8999);
            Assert.AreEqual(settings.DemoDouble, 1.7E+3);
            Assert.AreEqual(settings.DemoGuid, new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899"));
            Assert.AreEqual(settings.DemoInt, 17);
            Assert.AreEqual(settings.DemoLong, 9223372036854775807);
            Assert.AreEqual(settings.DemoString, "hello, I'm a string ");
            Assert.AreEqual(settings.DemoTimeSpanFirst, new TimeSpan(01, 02, 03));
            Assert.AreEqual(settings.DemoTimeSpanSecond, new TimeSpan(01, 02, 03, 04));

            Assert.AreEqual(settings.NumbersWithInitialRawValue, new List<int> { 1,2,3,4,5 });

            Assert.AreEqual(settings.DemoBooleans, new List<bool> { true, false, true, false, true, true, false });
            Assert.AreEqual(settings.DemoDateTimes, new List<DateTime> { new DateTime(2017, 11, 29, 23, 39, 03), new DateTime(2018, 11, 29, 15, 20, 03) });
            Assert.AreEqual(settings.DemoDecimals, new List<decimal> { -12336.89M, 1234.5M });
            Assert.AreEqual(settings.DemoDoubles, new List<double> { 1.7E+3, 1.5E+3 });
            Assert.AreEqual(settings.DemoGuids, new List<Guid> { new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899"), new Guid("4b865f54-6df4-4d0c-8a79-c07f33dddec4") });
            Assert.AreEqual(settings.Numbers, new List<int> { 1, 99, 123456789 });
            Assert.AreEqual(settings.DemoLongs, new List<long> { 9223372036854775807, 12345678 });
            Assert.AreEqual(settings.Strings, new List<string> { "luong ", "son", " ba ", "chuc anh dai  " });
            Assert.AreEqual(settings.DemoTimespans, new List<TimeSpan> { new TimeSpan(01, 02, 03), new TimeSpan(01, 02, 03, 04) });
        }
    }
}
