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

        [Option(Separator = ",")]
        IReadOnlyList<bool> DemoReadonlyBooleans { get; }

        DateTime DemoDateTime { get; }

        List<DateTime> DemoDateTimes { get; }
        IReadOnlyList<DateTime> DemoReadonlyDateTimes { get; }

        [Option(InputFormat = "dd+MM/yyyy")]
        DateTime DemoDateTimeWithFormat { get; }

        decimal DemoDecimal { get; }

        List<decimal> DemoDecimals { get; }
        IReadOnlyList<decimal> DemoReadonlyDecimals { get; }

        double DemoDouble { get; }
        List<double> DemoDoubles { get; }
        IReadOnlyList<double> DemoReadonlyDoubles { get; }

        Guid DemoGuid { get; }
        List<Guid> DemoGuids { get; }
        IReadOnlyList<Guid> DemoReadonlyGuids { get; }


        [Option(DefaultValue = 77)]
        int DemoInt { get; }

        [Option(Separator = "^")]
        List<int> Numbers { get; }

        [Option(Separator = "^")]
        IReadOnlyList<int> ReadonlyNumbers { get; }

        long DemoLong { get; }
        List<long> DemoLongs { get; }
        IReadOnlyList<long> DemoReadonlyLongs { get; }

        string DemoString { get; }

        [Option(Separator = "~")]
        List<string> Strings { get; }

        [Option(Separator = "~")]
        IReadOnlyList<string> ReadonlyStrings { get; }

        TimeSpan DemoTimeSpanFirst { get; }
        TimeSpan DemoTimeSpanSecond { get; }

        [Option(RawValue = "1;2;3;4;5", Separator = ";")]
        List<int> NumbersWithInitialRawValue { get; }

        List<TimeSpan> DemoTimespans { get; }
        IReadOnlyList<TimeSpan> DemoReadonlyTimespans { get; }
    }

    [TestFixture]
    [Description("Tests for parsing base types from App.config settings")]
    public class BaseTypeParserTest
    {
        private ITypeParserItemSetting settings;

        [SetUp]
        public void Setup()
        {
            settings = MyAppCfg.Get<ITypeParserItemSetting>();
        }

        #region Boolean Parsing Tests

        [Test]
        [Description("Verifies that boolean values are correctly parsed from App.config")]
        public void BooleanParser_SingleValue_ParsesCorrectly()
        {
            Assert.AreEqual(true, settings.DemoBoolean, "Boolean value should be parsed as true");
        }

        [Test]
        [Description("Verifies that a list of boolean values with comma separator is parsed correctly")]
        public void BooleanParser_ListWithCommaSeparator_ParsesCorrectly()
        {
            var expected = new List<bool> { true, false, true, false, true, true, false };
            Assert.AreEqual(expected, settings.DemoBooleans, "Boolean list should contain 7 values with correct true/false pattern");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<bool> values are parsed correctly")]
        public void BooleanParser_ReadOnlyList_ParsesCorrectly()
        {
            var expected = new List<bool> { true, false, true, false, true, true, false };
            Assert.AreEqual(expected, settings.DemoReadonlyBooleans, "ReadOnly boolean list should match expected values");
        }

        #endregion

        #region DateTime Parsing Tests

        [Test]
        [Description("Verifies that DateTime values are correctly parsed from App.config")]
        public void DateTimeParser_SingleValue_ParsesCorrectly()
        {
            var expected = new DateTime(2017, 11, 29, 23, 39, 03);
            Assert.AreEqual(expected, settings.DemoDateTime, "DateTime should be parsed as 2017-11-29 23:39:03");
        }

        [Test]
        [Description("Verifies that DateTime with custom input format (dd+MM/yyyy) is parsed correctly")]
        public void DateTimeParser_WithCustomFormat_ParsesCorrectly()
        {
            var expected = new DateTime(2015, 09, 24);
            Assert.AreEqual(expected, settings.DemoDateTimeWithFormat, "DateTime with format dd+MM/yyyy should be parsed as 2015-09-24");
        }

        [Test]
        [Description("Verifies that a list of DateTime values is parsed correctly")]
        public void DateTimeParser_List_ParsesCorrectly()
        {
            var expected = new List<DateTime> { new DateTime(2017, 11, 29, 23, 39, 03), new DateTime(2018, 11, 29, 15, 20, 03) };
            Assert.AreEqual(expected, settings.DemoDateTimes, "DateTime list should contain 2 values with correct dates");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<DateTime> values are parsed correctly")]
        public void DateTimeParser_ReadOnlyList_ParsesCorrectly()
        {
            var expected = new List<DateTime> { new DateTime(2017, 11, 29, 23, 39, 03), new DateTime(2018, 11, 29, 15, 20, 03) };
            Assert.AreEqual(expected, settings.DemoReadonlyDateTimes, "ReadOnly DateTime list should match expected values");
        }

        #endregion

        #region Decimal Parsing Tests

        [Test]
        [Description("Verifies that decimal values are correctly parsed from App.config")]
        public void DecimalParser_SingleValue_ParsesCorrectly()
        {
            Assert.AreEqual(-12336.8999m, settings.DemoDecimal, "Decimal value should be parsed as -12336.8999");
        }

        [Test]
        [Description("Verifies that a list of decimal values is parsed correctly")]
        public void DecimalParser_List_ParsesCorrectly()
        {
            var expected = new List<decimal> { -12336.89M, 1234.5M };
            Assert.AreEqual(expected, settings.DemoDecimals, "Decimal list should contain 2 values");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<decimal> values are parsed correctly")]
        public void DecimalParser_ReadOnlyList_ParsesCorrectly()
        {
            var expected = new List<decimal> { -12336.89M, 1234.5M };
            Assert.AreEqual(expected, settings.DemoReadonlyDecimals, "ReadOnly decimal list should match expected values");
        }

        #endregion

        #region Double Parsing Tests

        [Test]
        [Description("Verifies that double values with scientific notation are correctly parsed")]
        public void DoubleParser_ScientificNotation_ParsesCorrectly()
        {
            Assert.AreEqual(1.7E+3, settings.DemoDouble, "Double value should be parsed as 1.7E+3 (1700)");
        }

        [Test]
        [Description("Verifies that a list of double values is parsed correctly")]
        public void DoubleParser_List_ParsesCorrectly()
        {
            var expected = new List<double> { 1.7E+3, 1.5E+3 };
            Assert.AreEqual(expected, settings.DemoDoubles, "Double list should contain 2 values with scientific notation");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<double> values are parsed correctly")]
        public void DoubleParser_ReadOnlyList_ParsesCorrectly()
        {
            var expected = new List<double> { 1.7E+3, 1.5E+3 };
            Assert.AreEqual(expected, settings.DemoReadonlyDoubles, "ReadOnly double list should match expected values");
        }

        #endregion

        #region Guid Parsing Tests

        [Test]
        [Description("Verifies that Guid values are correctly parsed from App.config")]
        public void GuidParser_SingleValue_ParsesCorrectly()
        {
            var expected = new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899");
            Assert.AreEqual(expected, settings.DemoGuid, "Guid should be parsed correctly");
        }

        [Test]
        [Description("Verifies that a list of Guid values is parsed correctly")]
        public void GuidParser_List_ParsesCorrectly()
        {
            var expected = new List<Guid> { new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899"), new Guid("4b865f54-6df4-4d0c-8a79-c07f33dddec4") };
            Assert.AreEqual(expected, settings.DemoGuids, "Guid list should contain 2 values");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<Guid> values are parsed correctly")]
        public void GuidParser_ReadOnlyList_ParsesCorrectly()
        {
            var expected = new List<Guid> { new Guid("8ff3a01d-1884-4ebd-b787-d5980aa94899"), new Guid("4b865f54-6df4-4d0c-8a79-c07f33dddec4") };
            Assert.AreEqual(expected, settings.DemoReadonlyGuids, "ReadOnly Guid list should match expected values");
        }

        #endregion

        #region Integer Parsing Tests

        [Test]
        [Description("Verifies that integer values are correctly parsed from App.config")]
        public void IntParser_SingleValue_ParsesCorrectly()
        {
            Assert.AreEqual(17, settings.DemoInt, "Integer value should be parsed as 17");
        }

        [Test]
        [Description("Verifies that a list of integers with caret separator is parsed correctly")]
        public void IntParser_ListWithCaretSeparator_ParsesCorrectly()
        {
            var expected = new List<int> { 1, 99, 123456789 };
            Assert.AreEqual(expected, settings.Numbers, "Integer list with '^' separator should contain 3 values");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<int> values are parsed correctly")]
        public void IntParser_ReadOnlyList_ParsesCorrectly()
        {
            var expected = new List<int> { 1, 99, 123456789 };
            Assert.AreEqual(expected, settings.ReadonlyNumbers, "ReadOnly integer list should match expected values");
        }

        [Test]
        [Description("Verifies that RawValue attribute with semicolon separator is parsed correctly")]
        public void IntParser_RawValueWithSemicolonSeparator_ParsesCorrectly()
        {
            var expected = new List<int> { 1, 2, 3, 4, 5 };
            Assert.AreEqual(expected, settings.NumbersWithInitialRawValue, "RawValue '1;2;3;4;5' with ';' separator should parse to list [1,2,3,4,5]");
        }

        #endregion

        #region Long Parsing Tests

        [Test]
        [Description("Verifies that long values including max value are correctly parsed")]
        public void LongParser_MaxValue_ParsesCorrectly()
        {
            Assert.AreEqual(9223372036854775807, settings.DemoLong, "Long value should be parsed as max long value");
        }

        [Test]
        [Description("Verifies that a list of long values is parsed correctly")]
        public void LongParser_List_ParsesCorrectly()
        {
            var expected = new List<long> { 9223372036854775807, 12345678 };
            Assert.AreEqual(expected, settings.DemoLongs, "Long list should contain 2 values including max long value");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<long> values are parsed correctly")]
        public void LongParser_ReadOnlyList_ParsesCorrectly()
        {
            var expected = new List<long> { 9223372036854775807, 12345678 };
            Assert.AreEqual(expected, settings.DemoReadonlyLongs, "ReadOnly long list should match expected values");
        }

        #endregion

        #region String Parsing Tests

        [Test]
        [Description("Verifies that string values including whitespace are preserved correctly")]
        public void StringParser_SingleValue_PreservesWhitespace()
        {
            Assert.AreEqual("hello, I'm a string ", settings.DemoString, "String value should preserve trailing whitespace");
        }

        [Test]
        [Description("Verifies that a list of strings with tilde separator preserves whitespace")]
        public void StringParser_ListWithTildeSeparator_PreservesWhitespace()
        {
            var expected = new List<string> { "luong ", "son", " ba ", "chuc anh dai  " };
            Assert.AreEqual(expected, settings.Strings, "String list with '~' separator should preserve whitespace in each element");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<string> values preserve whitespace")]
        public void StringParser_ReadOnlyList_PreservesWhitespace()
        {
            var expected = new List<string> { "luong ", "son", " ba ", "chuc anh dai  " };
            Assert.AreEqual(expected, settings.ReadonlyStrings, "ReadOnly string list should preserve whitespace");
        }

        #endregion

        #region TimeSpan Parsing Tests

        [Test]
        [Description("Verifies that TimeSpan values in HH:MM:SS format are parsed correctly")]
        public void TimeSpanParser_HoursMinutesSeconds_ParsesCorrectly()
        {
            var expected = new TimeSpan(01, 02, 03);
            Assert.AreEqual(expected, settings.DemoTimeSpanFirst, "TimeSpan should be parsed as 01:02:03");
        }

        [Test]
        [Description("Verifies that TimeSpan values in DD:HH:MM:SS format are parsed correctly")]
        public void TimeSpanParser_DaysHoursMinutesSeconds_ParsesCorrectly()
        {
            var expected = new TimeSpan(01, 02, 03, 04);
            Assert.AreEqual(expected, settings.DemoTimeSpanSecond, "TimeSpan should be parsed as 1 day, 02:03:04");
        }

        [Test]
        [Description("Verifies that a list of TimeSpan values is parsed correctly")]
        public void TimeSpanParser_List_ParsesCorrectly()
        {
            var expected = new List<TimeSpan> { new TimeSpan(01, 02, 03), new TimeSpan(01, 02, 03, 04) };
            Assert.AreEqual(expected, settings.DemoTimespans, "TimeSpan list should contain 2 values with different formats");
        }

        [Test]
        [Description("Verifies that IReadOnlyList<TimeSpan> values are parsed correctly")]
        public void TimeSpanParser_ReadOnlyList_ParsesCorrectly()
        {
            var expected = new List<TimeSpan> { new TimeSpan(01, 02, 03), new TimeSpan(01, 02, 03, 04) };
            Assert.AreEqual(expected, settings.DemoReadonlyTimespans, "ReadOnly TimeSpan list should match expected values");
        }

        #endregion
    }
}
