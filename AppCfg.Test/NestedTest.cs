using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    public interface IRootBaseSetting
    {
        bool DemoBoolean { get; }
        DateTime DemoDateTime { get; }

        [Option(InputFormat = "dd+MM/yyyy")]
        DateTime DemoDateTimeWithFormat { get; }

        decimal DemoDecimal { get; }
        double DemoDouble { get; }
        Guid DemoGuid { get; }

        [Option(DefaultValue = 77)]
        int DemoInt { get; }

        long DemoLong { get; }

        string DemoString { get; }
        TimeSpan DemoTimeSpanFirst { get; }
        TimeSpan DemoTimeSpanSecond { get; }

        [Option(Separator = "^")]
        List<int> Numbers { get; }

        [Option(RawValue = "1;2;3;4;5", Separator = ";")]
        List<int> NumbersWithInitialRawValue { get; }

        [Option(Separator = "~")]
        List<string> Strings { get; }

        INestedBaseSetting NestedSettings { get; }
    }

    public interface INestedBaseSetting
    {
        bool DemoBoolean { get; }
        DateTime DemoDateTime { get; }

        [Option(InputFormat = "dd+MM/yyyy")]
        DateTime DemoDateTimeWithFormat { get; }

        decimal DemoDecimal { get; }
        double DemoDouble { get; }
        Guid DemoGuid { get; }

        [Option(DefaultValue = 77)]
        int DemoInt { get; }

        long DemoLong { get; }

        string DemoString { get; }
        TimeSpan DemoTimeSpanFirst { get; }
        TimeSpan DemoTimeSpanSecond { get; }

        [Option(Separator = "^")]
        List<int> Numbers { get; }

        [Option(RawValue = "1;2;3;4;5", Separator = ";")]
        List<int> NumbersWithInitialRawValue { get; }

        [Option(Separator = "~")]
        List<string> Strings { get; }
    }

    [TestFixture]
    public class NestedTest
    {        
        [Test]
        public void NestedConfigTest()
        {
            IRootBaseSetting settings = MyAppCfg.Get<IRootBaseSetting>();
            
            DoRootTest(settings);
            DoNestedTest(settings.NestedSettings);
        }

        private void DoRootTest(IRootBaseSetting setting)
        {
            Assert.AreEqual(setting.DemoBoolean, true);
            Assert.AreEqual(setting.DemoDateTime, new DateTime(2017, 11, 29, 23, 39, 03));
            Assert.AreEqual(setting.DemoDateTimeWithFormat, new DateTime(2015, 09, 24));
            Assert.AreEqual(setting.DemoDecimal, -12336.8999);
            Assert.AreEqual(setting.DemoDouble, 1.7E+3);
            Assert.AreEqual(setting.DemoGuid, new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899"));
            Assert.AreEqual(setting.DemoInt, 17);
            Assert.AreEqual(setting.DemoLong, 9223372036854775807);
            Assert.AreEqual(setting.DemoString, "hello, I'm a string ");
            Assert.AreEqual(setting.DemoTimeSpanFirst, new TimeSpan(01, 02, 03));
            Assert.AreEqual(setting.DemoTimeSpanSecond, new TimeSpan(01, 02, 03, 04));
                            
            Assert.AreEqual(setting.Numbers, new List<int> { 1, 99, 123456789 });
            Assert.AreEqual(setting.NumbersWithInitialRawValue, new List<int> { 1, 2, 3, 4, 5 });
            Assert.AreEqual(setting.Strings, new List<string> { "luong ", "son", " ba ", "chuc anh dai  " });
        }

        private void DoNestedTest(INestedBaseSetting setting)
        {
            Assert.AreEqual(setting.DemoBoolean, true);
            Assert.AreEqual(setting.DemoDateTime, new DateTime(2017, 11, 29, 23, 39, 03));
            Assert.AreEqual(setting.DemoDateTimeWithFormat, new DateTime(2015, 09, 24));
            Assert.AreEqual(setting.DemoDecimal, -12336.8999);
            Assert.AreEqual(setting.DemoDouble, 1.7E+3);
            Assert.AreEqual(setting.DemoGuid, new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899"));
            Assert.AreEqual(setting.DemoInt, 17);
            Assert.AreEqual(setting.DemoLong, 9223372036854775807);
            Assert.AreEqual(setting.DemoString, "hello, I'm a string ");
            Assert.AreEqual(setting.DemoTimeSpanFirst, new TimeSpan(01, 02, 03));
            Assert.AreEqual(setting.DemoTimeSpanSecond, new TimeSpan(01, 02, 03, 04));
                            
            Assert.AreEqual(setting.Numbers, new List<int> { 1, 99, 123456789 });
            Assert.AreEqual(setting.NumbersWithInitialRawValue, new List<int> { 1, 2, 3, 4, 5 });
            Assert.AreEqual(setting.Strings, new List<string> { "luong ", "son", " ba ", "chuc anh dai  " });
        }
    }
}
