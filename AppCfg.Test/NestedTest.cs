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
    [Description("Tests for nested configuration interface support")]
    public class NestedTest
    {
        private IRootBaseSetting settings;

        [SetUp]
        public void Setup()
        {
            settings = MyAppCfg.Get<IRootBaseSetting>();
        }

        #region Root Level Settings Tests

        [Test]
        [Description("Verifies that root level boolean is parsed correctly")]
        public void RootSettings_Boolean_ParsesCorrectly()
        {
            Assert.AreEqual(true, settings.DemoBoolean, "Root boolean should be true");
        }

        [Test]
        [Description("Verifies that root level DateTime is parsed correctly")]
        public void RootSettings_DateTime_ParsesCorrectly()
        {
            var expected = new DateTime(2017, 11, 29, 23, 39, 03);
            Assert.AreEqual(expected, settings.DemoDateTime, "Root DateTime should be 2017-11-29 23:39:03");
        }

        [Test]
        [Description("Verifies that root level DateTime with custom format is parsed correctly")]
        public void RootSettings_DateTimeWithFormat_ParsesCorrectly()
        {
            var expected = new DateTime(2015, 09, 24);
            Assert.AreEqual(expected, settings.DemoDateTimeWithFormat, "Root DateTime with format should be 2015-09-24");
        }

        [Test]
        [Description("Verifies that root level decimal is parsed correctly")]
        public void RootSettings_Decimal_ParsesCorrectly()
        {
            Assert.AreEqual(-12336.8999m, settings.DemoDecimal, "Root decimal should be -12336.8999");
        }

        [Test]
        [Description("Verifies that root level double is parsed correctly")]
        public void RootSettings_Double_ParsesCorrectly()
        {
            Assert.AreEqual(1.7E+3, settings.DemoDouble, "Root double should be 1.7E+3");
        }

        [Test]
        [Description("Verifies that root level Guid is parsed correctly")]
        public void RootSettings_Guid_ParsesCorrectly()
        {
            var expected = new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899");
            Assert.AreEqual(expected, settings.DemoGuid, "Root Guid should match expected value");
        }

        [Test]
        [Description("Verifies that root level integer is parsed correctly")]
        public void RootSettings_Integer_ParsesCorrectly()
        {
            Assert.AreEqual(17, settings.DemoInt, "Root integer should be 17");
        }

        [Test]
        [Description("Verifies that root level long is parsed correctly")]
        public void RootSettings_Long_ParsesCorrectly()
        {
            Assert.AreEqual(9223372036854775807, settings.DemoLong, "Root long should be max long value");
        }

        [Test]
        [Description("Verifies that root level string preserves whitespace")]
        public void RootSettings_String_PreservesWhitespace()
        {
            Assert.AreEqual("hello, I'm a string ", settings.DemoString, "Root string should preserve trailing whitespace");
        }

        [Test]
        [Description("Verifies that root level TimeSpan in HH:MM:SS format is parsed correctly")]
        public void RootSettings_TimeSpanFirst_ParsesCorrectly()
        {
            var expected = new TimeSpan(01, 02, 03);
            Assert.AreEqual(expected, settings.DemoTimeSpanFirst, "Root TimeSpan should be 01:02:03");
        }

        [Test]
        [Description("Verifies that root level TimeSpan in DD:HH:MM:SS format is parsed correctly")]
        public void RootSettings_TimeSpanSecond_ParsesCorrectly()
        {
            var expected = new TimeSpan(01, 02, 03, 04);
            Assert.AreEqual(expected, settings.DemoTimeSpanSecond, "Root TimeSpan should be 1 day, 02:03:04");
        }

        [Test]
        [Description("Verifies that root level integer list with caret separator is parsed correctly")]
        public void RootSettings_IntegerList_ParsesCorrectly()
        {
            var expected = new List<int> { 1, 99, 123456789 };
            Assert.AreEqual(expected, settings.Numbers, "Root integer list should contain 3 values");
        }

        [Test]
        [Description("Verifies that root level RawValue integer list is parsed correctly")]
        public void RootSettings_RawValueList_ParsesCorrectly()
        {
            var expected = new List<int> { 1, 2, 3, 4, 5 };
            Assert.AreEqual(expected, settings.NumbersWithInitialRawValue, "Root RawValue list should contain values 1-5");
        }

        [Test]
        [Description("Verifies that root level string list with tilde separator is parsed correctly")]
        public void RootSettings_StringList_ParsesCorrectly()
        {
            var expected = new List<string> { "luong ", "son", " ba ", "chuc anh dai  " };
            Assert.AreEqual(expected, settings.Strings, "Root string list should preserve whitespace in each element");
        }

        #endregion

        #region Nested Level Settings Tests

        [Test]
        [Description("Verifies that nested interface is not null")]
        public void NestedSettings_Interface_IsNotNull()
        {
            Assert.IsNotNull(settings.NestedSettings, "Nested settings interface should not be null");
        }

        [Test]
        [Description("Verifies that nested level boolean is parsed correctly")]
        public void NestedSettings_Boolean_ParsesCorrectly()
        {
            Assert.AreEqual(true, settings.NestedSettings.DemoBoolean, "Nested boolean should be true");
        }

        [Test]
        [Description("Verifies that nested level DateTime is parsed correctly")]
        public void NestedSettings_DateTime_ParsesCorrectly()
        {
            var expected = new DateTime(2017, 11, 29, 23, 39, 03);
            Assert.AreEqual(expected, settings.NestedSettings.DemoDateTime, "Nested DateTime should be 2017-11-29 23:39:03");
        }

        [Test]
        [Description("Verifies that nested level DateTime with custom format is parsed correctly")]
        public void NestedSettings_DateTimeWithFormat_ParsesCorrectly()
        {
            var expected = new DateTime(2015, 09, 24);
            Assert.AreEqual(expected, settings.NestedSettings.DemoDateTimeWithFormat, "Nested DateTime with format should be 2015-09-24");
        }

        [Test]
        [Description("Verifies that nested level decimal is parsed correctly")]
        public void NestedSettings_Decimal_ParsesCorrectly()
        {
            Assert.AreEqual(-12336.8999m, settings.NestedSettings.DemoDecimal, "Nested decimal should be -12336.8999");
        }

        [Test]
        [Description("Verifies that nested level double is parsed correctly")]
        public void NestedSettings_Double_ParsesCorrectly()
        {
            Assert.AreEqual(1.7E+3, settings.NestedSettings.DemoDouble, "Nested double should be 1.7E+3");
        }

        [Test]
        [Description("Verifies that nested level Guid is parsed correctly")]
        public void NestedSettings_Guid_ParsesCorrectly()
        {
            var expected = new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899");
            Assert.AreEqual(expected, settings.NestedSettings.DemoGuid, "Nested Guid should match expected value");
        }

        [Test]
        [Description("Verifies that nested level integer is parsed correctly")]
        public void NestedSettings_Integer_ParsesCorrectly()
        {
            Assert.AreEqual(17, settings.NestedSettings.DemoInt, "Nested integer should be 17");
        }

        [Test]
        [Description("Verifies that nested level long is parsed correctly")]
        public void NestedSettings_Long_ParsesCorrectly()
        {
            Assert.AreEqual(9223372036854775807, settings.NestedSettings.DemoLong, "Nested long should be max long value");
        }

        [Test]
        [Description("Verifies that nested level string preserves whitespace")]
        public void NestedSettings_String_PreservesWhitespace()
        {
            Assert.AreEqual("hello, I'm a string ", settings.NestedSettings.DemoString, "Nested string should preserve trailing whitespace");
        }

        [Test]
        [Description("Verifies that nested level TimeSpan in HH:MM:SS format is parsed correctly")]
        public void NestedSettings_TimeSpanFirst_ParsesCorrectly()
        {
            var expected = new TimeSpan(01, 02, 03);
            Assert.AreEqual(expected, settings.NestedSettings.DemoTimeSpanFirst, "Nested TimeSpan should be 01:02:03");
        }

        [Test]
        [Description("Verifies that nested level TimeSpan in DD:HH:MM:SS format is parsed correctly")]
        public void NestedSettings_TimeSpanSecond_ParsesCorrectly()
        {
            var expected = new TimeSpan(01, 02, 03, 04);
            Assert.AreEqual(expected, settings.NestedSettings.DemoTimeSpanSecond, "Nested TimeSpan should be 1 day, 02:03:04");
        }

        [Test]
        [Description("Verifies that nested level integer list with caret separator is parsed correctly")]
        public void NestedSettings_IntegerList_ParsesCorrectly()
        {
            var expected = new List<int> { 1, 99, 123456789 };
            Assert.AreEqual(expected, settings.NestedSettings.Numbers, "Nested integer list should contain 3 values");
        }

        [Test]
        [Description("Verifies that nested level RawValue integer list is parsed correctly")]
        public void NestedSettings_RawValueList_ParsesCorrectly()
        {
            var expected = new List<int> { 1, 2, 3, 4, 5 };
            Assert.AreEqual(expected, settings.NestedSettings.NumbersWithInitialRawValue, "Nested RawValue list should contain values 1-5");
        }

        [Test]
        [Description("Verifies that nested level string list with tilde separator is parsed correctly")]
        public void NestedSettings_StringList_ParsesCorrectly()
        {
            var expected = new List<string> { "luong ", "son", " ba ", "chuc anh dai  " };
            Assert.AreEqual(expected, settings.NestedSettings.Strings, "Nested string list should preserve whitespace in each element");
        }

        #endregion
    }
}
