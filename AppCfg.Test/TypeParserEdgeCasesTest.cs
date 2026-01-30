using AppCfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    /// <summary>
    /// Tests for edge cases in type parsing - whitespace, special characters,
    /// boundary values, and various format variations.
    /// </summary>
    [TestFixture]
    public class TypeParserEdgeCasesTest
    {
        private const string TestStoreKey = "TypeParserEdgeCases";

        [SetUp]
        public void Setup()
        {
            // Register a custom store for testing various edge cases
            MyAppCfg.SettingStores.RegisterStore(TestStoreKey, metadata =>
            {
                var data = new Dictionary<string, string>
                {
                    // String edge cases
                    ["EmptyString"] = "",
                    ["WhitespaceOnly"] = "   ",
                    ["StringWithNewlines"] = "line1\nline2\r\nline3",
                    ["StringWithTabs"] = "col1\tcol2\tcol3",
                    ["StringWithSpecialChars"] = "hello <world> & \"friends\"",
                    ["UnicodeString"] = "Hello 世界 🌍",

                    // Integer edge cases
                    ["IntZero"] = "0",
                    ["IntNegative"] = "-42",
                    ["IntMaxValue"] = int.MaxValue.ToString(),
                    ["IntMinValue"] = int.MinValue.ToString(),
                    ["IntWithSpaces"] = " 123 ",
                    ["IntWithCommas"] = "1,234,567",

                    // Long edge cases
                    ["LongMaxValue"] = long.MaxValue.ToString(),
                    ["LongMinValue"] = long.MinValue.ToString(),

                    // Double edge cases
                    ["DoubleZero"] = "0.0",
                    ["DoubleNegative"] = "-123.456",
                    ["DoubleScientific"] = "1.23E+10",
                    ["DoubleVerySmall"] = "0.000001",

                    // Boolean edge cases
                    ["BoolTrue"] = "true",
                    ["BoolFalse"] = "false",
                    ["BoolOne"] = "1",
                    ["BoolZero"] = "0",
                    ["BoolYes"] = "yes",
                    ["BoolNo"] = "no",
                    ["BoolTrueUpperCase"] = "TRUE",
                    ["BoolFalseMixedCase"] = "FaLsE",

                    // Guid edge cases
                    ["GuidWithBraces"] = "{12345678-1234-1234-1234-123456789abc}",
                    ["GuidWithoutBraces"] = "12345678-1234-1234-1234-123456789abc",
                    ["GuidUpperCase"] = "12345678-1234-1234-1234-123456789ABC",
                    ["GuidEmpty"] = "00000000-0000-0000-0000-000000000000",

                    // TimeSpan edge cases
                    ["TimeSpanShort"] = "01:30",
                    ["TimeSpanWithSeconds"] = "01:30:45",
                    ["TimeSpanWithDays"] = "5.01:30:45",
                    ["TimeSpanNegative"] = "-01:30:00",
                    ["TimeSpanZero"] = "00:00:00",

                    // List edge cases
                    ["ListSingleItem"] = "single",
                    ["ListWithEmpty"] = "a;;b",
                    ["ListManyItems"] = "1;2;3;4;5;6;7;8;9;10"
                };

                return data.TryGetValue(metadata.SettingKey, out var value) ? value : null;
            });
        }

        #region String Edge Cases

        /// <summary>
        /// Verifies that an empty string configuration value is parsed and returned as an empty string.
        /// </summary>
        [Test, Description("Empty string value should be parsed as empty string")]
        public void String_Empty_ReturnsEmptyString()
        {
            var settings = MyAppCfg.Get<IStringEdgeCases>();
            Assert.AreEqual("", settings.EmptyString, "Empty string should remain empty after parsing");
        }

        /// <summary>
        /// Verifies that whitespace-only strings are preserved without trimming.
        /// </summary>
        [Test, Description("Whitespace-only string should be preserved as-is")]
        public void String_WhitespaceOnly_PreservesWhitespace()
        {
            var settings = MyAppCfg.Get<IStringEdgeCases>();
            Assert.AreEqual("   ", settings.WhitespaceOnly, "Whitespace should be preserved");
        }

        /// <summary>
        /// Verifies that newline characters in strings are preserved during parsing.
        /// </summary>
        [Test, Description("String with newlines should preserve newline characters")]
        public void String_WithNewlines_PreservesNewlines()
        {
            var settings = MyAppCfg.Get<IStringEdgeCases>();
            Assert.That(settings.StringWithNewlines, Does.Contain("\n"), "Newline characters should be preserved");
        }

        /// <summary>
        /// Verifies that special characters (angle brackets, ampersand, quotes) are preserved.
        /// </summary>
        [Test, Description("String with special characters (<, >, &, \") should be preserved")]
        public void String_WithSpecialChars_PreservesSpecialChars()
        {
            var settings = MyAppCfg.Get<IStringEdgeCases>();
            Assert.AreEqual("hello <world> & \"friends\"", settings.StringWithSpecialChars,
                "Special characters should not be escaped or modified");
        }

        /// <summary>
        /// Verifies that Unicode characters including Chinese and emoji are preserved.
        /// </summary>
        [Test, Description("String with Unicode characters (Chinese, emoji) should be preserved")]
        public void String_Unicode_PreservesUnicode()
        {
            var settings = MyAppCfg.Get<IStringEdgeCases>();
            Assert.That(settings.UnicodeString, Does.Contain("世界"), "Chinese characters should be preserved");
            Assert.That(settings.UnicodeString, Does.Contain("🌍"), "Emoji should be preserved");
        }

        #endregion

        #region Integer Edge Cases

        /// <summary>
        /// Verifies that zero is correctly parsed as integer 0.
        /// </summary>
        [Test, Description("String '0' should be parsed as integer 0")]
        public void Int_Zero_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IIntEdgeCases>();
            Assert.AreEqual(0, settings.IntZero, "Zero should parse correctly");
        }

        /// <summary>
        /// Verifies that negative integers are correctly parsed.
        /// </summary>
        [Test, Description("Negative integer string '-42' should be parsed as -42")]
        public void Int_Negative_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IIntEdgeCases>();
            Assert.AreEqual(-42, settings.IntNegative, "Negative number should parse correctly");
        }

        /// <summary>
        /// Verifies that Int32.MaxValue (2,147,483,647) is correctly parsed.
        /// </summary>
        [Test, Description("Int32.MaxValue should be parsed correctly")]
        public void Int_MaxValue_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IIntEdgeCases>();
            Assert.AreEqual(int.MaxValue, settings.IntMaxValue, "Int32.MaxValue should parse correctly");
        }

        /// <summary>
        /// Verifies that Int32.MinValue (-2,147,483,648) is correctly parsed.
        /// </summary>
        [Test, Description("Int32.MinValue should be parsed correctly")]
        public void Int_MinValue_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IIntEdgeCases>();
            Assert.AreEqual(int.MinValue, settings.IntMinValue, "Int32.MinValue should parse correctly");
        }

        /// <summary>
        /// Verifies that integers with leading/trailing spaces are trimmed and parsed correctly.
        /// </summary>
        [Test, Description("Integer with leading/trailing spaces ' 123 ' should be parsed as 123")]
        public void Int_WithSpaces_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IIntEdgeCases>();
            Assert.AreEqual(123, settings.IntWithSpaces, "Spaces should be trimmed before parsing");
        }

        /// <summary>
        /// Verifies that integers with thousand separators (commas) are correctly parsed.
        /// </summary>
        [Test, Description("Integer with commas '1,234,567' should be parsed as 1234567")]
        public void Int_WithCommas_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IIntEdgeCases>();
            Assert.AreEqual(1234567, settings.IntWithCommas, "Commas should be handled as thousand separators");
        }

        #endregion

        #region Long Edge Cases

        /// <summary>
        /// Verifies that Int64.MaxValue (9,223,372,036,854,775,807) is correctly parsed.
        /// </summary>
        [Test, Description("Int64.MaxValue should be parsed correctly")]
        public void Long_MaxValue_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<ILongEdgeCases>();
            Assert.AreEqual(long.MaxValue, settings.LongMaxValue, "Int64.MaxValue should parse correctly");
        }

        /// <summary>
        /// Verifies that Int64.MinValue (-9,223,372,036,854,775,808) is correctly parsed.
        /// </summary>
        [Test, Description("Int64.MinValue should be parsed correctly")]
        public void Long_MinValue_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<ILongEdgeCases>();
            Assert.AreEqual(long.MinValue, settings.LongMinValue, "Int64.MinValue should parse correctly");
        }

        #endregion

        #region Double Edge Cases

        /// <summary>
        /// Verifies that '0.0' is correctly parsed as double 0.0.
        /// </summary>
        [Test, Description("String '0.0' should be parsed as double 0.0")]
        public void Double_Zero_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IDoubleEdgeCases>();
            Assert.AreEqual(0.0, settings.DoubleZero, "Zero should parse correctly");
        }

        /// <summary>
        /// Verifies that negative doubles are correctly parsed.
        /// </summary>
        [Test, Description("Negative double '-123.456' should be parsed correctly")]
        public void Double_Negative_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IDoubleEdgeCases>();
            Assert.AreEqual(-123.456, settings.DoubleNegative, 0.0001, "Negative double should parse correctly");
        }

        /// <summary>
        /// Verifies that scientific notation (e.g., '1.23E+10') is correctly parsed.
        /// </summary>
        [Test, Description("Scientific notation '1.23E+10' should be parsed as 12,300,000,000")]
        public void Double_ScientificNotation_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IDoubleEdgeCases>();
            Assert.AreEqual(1.23E+10, settings.DoubleScientific, 1E+5, "Scientific notation should parse correctly");
        }

        /// <summary>
        /// Verifies that very small doubles (e.g., '0.000001') maintain precision.
        /// </summary>
        [Test, Description("Very small double '0.000001' should maintain precision")]
        public void Double_VerySmall_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IDoubleEdgeCases>();
            Assert.AreEqual(0.000001, settings.DoubleVerySmall, 0.0000001, "Small double should maintain precision");
        }

        #endregion

        #region Boolean Edge Cases

        /// <summary>
        /// Verifies that 'true' (lowercase) is parsed as boolean true.
        /// </summary>
        [Test, Description("String 'true' should be parsed as boolean true")]
        public void Bool_True_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IBoolEdgeCases>();
            Assert.IsTrue(settings.BoolTrue, "'true' should parse as true");
        }

        /// <summary>
        /// Verifies that 'false' (lowercase) is parsed as boolean false.
        /// </summary>
        [Test, Description("String 'false' should be parsed as boolean false")]
        public void Bool_False_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IBoolEdgeCases>();
            Assert.IsFalse(settings.BoolFalse, "'false' should parse as false");
        }

        /// <summary>
        /// Verifies that '1' is parsed as boolean true.
        /// </summary>
        [Test, Description("String '1' should be parsed as boolean true")]
        public void Bool_One_ParsesAsTrue()
        {
            var settings = MyAppCfg.Get<IBoolEdgeCases>();
            Assert.IsTrue(settings.BoolOne, "'1' should parse as true");
        }

        /// <summary>
        /// Verifies that '0' is parsed as boolean false.
        /// </summary>
        [Test, Description("String '0' should be parsed as boolean false")]
        public void Bool_Zero_ParsesAsFalse()
        {
            var settings = MyAppCfg.Get<IBoolEdgeCases>();
            Assert.IsFalse(settings.BoolZero, "'0' should parse as false");
        }

        /// <summary>
        /// Verifies that 'yes' is parsed as boolean true.
        /// </summary>
        [Test, Description("String 'yes' should be parsed as boolean true")]
        public void Bool_Yes_ParsesAsTrue()
        {
            var settings = MyAppCfg.Get<IBoolEdgeCases>();
            Assert.IsTrue(settings.BoolYes, "'yes' should parse as true");
        }

        /// <summary>
        /// Verifies that 'no' is parsed as boolean false.
        /// </summary>
        [Test, Description("String 'no' should be parsed as boolean false")]
        public void Bool_No_ParsesAsFalse()
        {
            var settings = MyAppCfg.Get<IBoolEdgeCases>();
            Assert.IsFalse(settings.BoolNo, "'no' should parse as false");
        }

        /// <summary>
        /// Verifies that 'TRUE' (uppercase) is parsed as boolean true (case-insensitive).
        /// </summary>
        [Test, Description("String 'TRUE' (uppercase) should be parsed as boolean true")]
        public void Bool_UpperCase_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IBoolEdgeCases>();
            Assert.IsTrue(settings.BoolTrueUpperCase, "'TRUE' should parse as true (case-insensitive)");
        }

        /// <summary>
        /// Verifies that 'FaLsE' (mixed case) is parsed as boolean false (case-insensitive).
        /// </summary>
        [Test, Description("String 'FaLsE' (mixed case) should be parsed as boolean false")]
        public void Bool_MixedCase_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IBoolEdgeCases>();
            Assert.IsFalse(settings.BoolFalseMixedCase, "'FaLsE' should parse as false (case-insensitive)");
        }

        #endregion

        #region Guid Edge Cases

        /// <summary>
        /// Verifies that GUID with braces '{...}' is correctly parsed.
        /// </summary>
        [Test, Description("GUID with braces '{12345678-...}' should be parsed correctly")]
        public void Guid_WithBraces_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IGuidEdgeCases>();
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-123456789abc"), settings.GuidWithBraces,
                "GUID with braces should parse correctly");
        }

        /// <summary>
        /// Verifies that GUID without braces is correctly parsed.
        /// </summary>
        [Test, Description("GUID without braces '12345678-...' should be parsed correctly")]
        public void Guid_WithoutBraces_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IGuidEdgeCases>();
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-123456789abc"), settings.GuidWithoutBraces,
                "GUID without braces should parse correctly");
        }

        /// <summary>
        /// Verifies that GUID with uppercase hex characters is correctly parsed.
        /// </summary>
        [Test, Description("GUID with uppercase hex 'ABC' should be parsed correctly")]
        public void Guid_UpperCase_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IGuidEdgeCases>();
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-123456789ABC"), settings.GuidUpperCase,
                "GUID with uppercase should parse correctly");
        }

        /// <summary>
        /// Verifies that empty GUID (all zeros) is parsed as Guid.Empty.
        /// </summary>
        [Test, Description("Empty GUID '00000000-0000-0000-0000-000000000000' should be parsed as Guid.Empty")]
        public void Guid_Empty_ParsesAsEmptyGuid()
        {
            var settings = MyAppCfg.Get<IGuidEdgeCases>();
            Assert.AreEqual(Guid.Empty, settings.GuidEmpty, "Empty GUID should parse as Guid.Empty");
        }

        #endregion

        #region TimeSpan Edge Cases

        /// <summary>
        /// Verifies that short format '01:30' (hours:minutes) is parsed as 1 hour 30 minutes.
        /// </summary>
        [Test, Description("TimeSpan '01:30' should be parsed as 1 hour 30 minutes (90 minutes)")]
        public void TimeSpan_Short_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<ITimeSpanEdgeCases>();
            Assert.AreEqual(new TimeSpan(1, 30, 0), settings.TimeSpanShort,
                "'01:30' should parse as 1 hour 30 minutes");
        }

        /// <summary>
        /// Verifies that '01:30:45' (hours:minutes:seconds) is correctly parsed.
        /// </summary>
        [Test, Description("TimeSpan '01:30:45' should be parsed as 1h 30m 45s")]
        public void TimeSpan_WithSeconds_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<ITimeSpanEdgeCases>();
            Assert.AreEqual(new TimeSpan(1, 30, 45), settings.TimeSpanWithSeconds,
                "'01:30:45' should parse correctly");
        }

        /// <summary>
        /// Verifies that '5.01:30:45' (days.hours:minutes:seconds) is correctly parsed.
        /// </summary>
        [Test, Description("TimeSpan '5.01:30:45' should be parsed as 5 days, 1h 30m 45s")]
        public void TimeSpan_WithDays_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<ITimeSpanEdgeCases>();
            Assert.AreEqual(new TimeSpan(5, 1, 30, 45), settings.TimeSpanWithDays,
                "'5.01:30:45' should parse as 5 days 1h 30m 45s");
        }

        /// <summary>
        /// Verifies that negative TimeSpan '-01:30:00' is correctly parsed.
        /// </summary>
        [Test, Description("Negative TimeSpan '-01:30:00' should be parsed as -1h 30m")]
        public void TimeSpan_Negative_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<ITimeSpanEdgeCases>();
            Assert.AreEqual(new TimeSpan(-1, -30, 0), settings.TimeSpanNegative,
                "Negative timespan should parse correctly");
        }

        /// <summary>
        /// Verifies that '00:00:00' is parsed as TimeSpan.Zero.
        /// </summary>
        [Test, Description("TimeSpan '00:00:00' should be parsed as TimeSpan.Zero")]
        public void TimeSpan_Zero_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<ITimeSpanEdgeCases>();
            Assert.AreEqual(TimeSpan.Zero, settings.TimeSpanZero, "Zero timespan should parse as TimeSpan.Zero");
        }

        #endregion

        #region List Edge Cases

        /// <summary>
        /// Verifies that a list with single item (no separator) is parsed as a list with one element.
        /// </summary>
        [Test, Description("Single item 'single' should be parsed as list with one element")]
        public void List_SingleItem_ParsesCorrectly()
        {
            var settings = MyAppCfg.Get<IListEdgeCases>();
            Assert.AreEqual(1, settings.ListSingleItem.Count, "Should have exactly one item");
            Assert.AreEqual("single", settings.ListSingleItem[0], "Item should be 'single'");
        }

        /// <summary>
        /// Verifies that empty elements between separators ('a;;b') are preserved in the list.
        /// </summary>
        [Test, Description("List 'a;;b' should include empty element between 'a' and 'b'")]
        public void List_WithEmptyElements_IncludesEmptyElements()
        {
            var settings = MyAppCfg.Get<IListEdgeCases>();
            Assert.AreEqual(3, settings.ListWithEmpty.Count, "Should have 3 items including empty");
            Assert.AreEqual("a", settings.ListWithEmpty[0], "First item should be 'a'");
            Assert.AreEqual("", settings.ListWithEmpty[1], "Second item should be empty string");
            Assert.AreEqual("b", settings.ListWithEmpty[2], "Third item should be 'b'");
        }

        /// <summary>
        /// Verifies that a list with 10 items is fully parsed with correct values.
        /// </summary>
        [Test, Description("List '1;2;3;...;10' should be parsed as list of 10 integers")]
        public void List_ManyItems_ParsesAllCorrectly()
        {
            var settings = MyAppCfg.Get<IListEdgeCases>();
            Assert.AreEqual(10, settings.ListManyItems.Count, "Should have 10 items");
            Assert.AreEqual(1, settings.ListManyItems[0], "First item should be 1");
            Assert.AreEqual(10, settings.ListManyItems[9], "Last item should be 10");
        }

        #endregion

        #region Test Interfaces

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IStringEdgeCases
        {
            [Option(Alias = "EmptyString")]
            string EmptyString { get; }

            [Option(Alias = "WhitespaceOnly")]
            string WhitespaceOnly { get; }

            [Option(Alias = "StringWithNewlines")]
            string StringWithNewlines { get; }

            [Option(Alias = "StringWithSpecialChars")]
            string StringWithSpecialChars { get; }

            [Option(Alias = "UnicodeString")]
            string UnicodeString { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IIntEdgeCases
        {
            [Option(Alias = "IntZero")]
            int IntZero { get; }

            [Option(Alias = "IntNegative")]
            int IntNegative { get; }

            [Option(Alias = "IntMaxValue")]
            int IntMaxValue { get; }

            [Option(Alias = "IntMinValue")]
            int IntMinValue { get; }

            [Option(Alias = "IntWithSpaces")]
            int IntWithSpaces { get; }

            [Option(Alias = "IntWithCommas")]
            int IntWithCommas { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface ILongEdgeCases
        {
            [Option(Alias = "LongMaxValue")]
            long LongMaxValue { get; }

            [Option(Alias = "LongMinValue")]
            long LongMinValue { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IDoubleEdgeCases
        {
            [Option(Alias = "DoubleZero")]
            double DoubleZero { get; }

            [Option(Alias = "DoubleNegative")]
            double DoubleNegative { get; }

            [Option(Alias = "DoubleScientific")]
            double DoubleScientific { get; }

            [Option(Alias = "DoubleVerySmall")]
            double DoubleVerySmall { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IBoolEdgeCases
        {
            [Option(Alias = "BoolTrue")]
            bool BoolTrue { get; }

            [Option(Alias = "BoolFalse")]
            bool BoolFalse { get; }

            [Option(Alias = "BoolOne")]
            bool BoolOne { get; }

            [Option(Alias = "BoolZero")]
            bool BoolZero { get; }

            [Option(Alias = "BoolYes")]
            bool BoolYes { get; }

            [Option(Alias = "BoolNo")]
            bool BoolNo { get; }

            [Option(Alias = "BoolTrueUpperCase")]
            bool BoolTrueUpperCase { get; }

            [Option(Alias = "BoolFalseMixedCase")]
            bool BoolFalseMixedCase { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IGuidEdgeCases
        {
            [Option(Alias = "GuidWithBraces")]
            Guid GuidWithBraces { get; }

            [Option(Alias = "GuidWithoutBraces")]
            Guid GuidWithoutBraces { get; }

            [Option(Alias = "GuidUpperCase")]
            Guid GuidUpperCase { get; }

            [Option(Alias = "GuidEmpty")]
            Guid GuidEmpty { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface ITimeSpanEdgeCases
        {
            [Option(Alias = "TimeSpanShort")]
            TimeSpan TimeSpanShort { get; }

            [Option(Alias = "TimeSpanWithSeconds")]
            TimeSpan TimeSpanWithSeconds { get; }

            [Option(Alias = "TimeSpanWithDays")]
            TimeSpan TimeSpanWithDays { get; }

            [Option(Alias = "TimeSpanNegative")]
            TimeSpan TimeSpanNegative { get; }

            [Option(Alias = "TimeSpanZero")]
            TimeSpan TimeSpanZero { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IListEdgeCases
        {
            [Option(Alias = "ListSingleItem", Separator = ";")]
            List<string> ListSingleItem { get; }

            [Option(Alias = "ListWithEmpty", Separator = ";")]
            List<string> ListWithEmpty { get; }

            [Option(Alias = "ListManyItems", Separator = ";")]
            List<int> ListManyItems { get; }
        }

        #endregion
    }
}
