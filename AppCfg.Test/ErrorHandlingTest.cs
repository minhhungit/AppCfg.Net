using AppCfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Tests for error handling and edge cases in configuration parsing")]
    public class ErrorHandlingTest
    {
        [SetUp]
        public void Setup()
        {
            // Note: Values are configured in App.config
            // ConfigurationManager.AppSettings is read-only at runtime
        }

        #region Missing Value Tests

        [Test]
        [Description("Verifies that missing string setting without default value returns null")]
        public void MissingRequiredSetting_WithoutDefault_ReturnsNull()
        {
            // Act
            var settings = MyAppCfg.Get<IMissingValueSettings>();

            // Assert
            Assert.IsNull(settings.MissingString, "Missing string without default should be null");
        }

        [Test]
        [Description("Verifies that missing string setting with default value returns the default")]
        public void MissingRequiredSetting_WithDefault_ReturnsDefault()
        {
            // Act
            var settings = MyAppCfg.Get<IMissingValueSettings>();

            // Assert
            Assert.AreEqual("default-value", settings.MissingStringWithDefault, "Missing string with default should return 'default-value'");
        }

        #endregion

        #region Type Conversion Error Tests

        [Test]
        [Description("Verifies that invalid integer value throws AppCfgException with property name in message")]
        public void InvalidIntTypeConversion_ThrowsAppCfgException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IInvalidIntSettings>();
                var value = settings.InvalidInt; // This should throw when parsing
            });

            Assert.That(ex.Message, Does.Contain("InvalidInt"), "Exception message should contain the property name 'InvalidInt'");
        }

        [Test]
        [Description("Verifies that invalid Guid format throws AppCfgException with property name in message")]
        public void InvalidGuidFormat_ThrowsAppCfgException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IInvalidGuidSettings>();
                var value = settings.InvalidGuid; // This should throw when parsing
            });

            Assert.That(ex.Message, Does.Contain("InvalidGuid"), "Exception message should contain the property name 'InvalidGuid'");
        }

        [Test]
        [Description("Verifies that invalid DateTime format throws AppCfgException with property name in message")]
        public void InvalidDateTimeFormat_ThrowsAppCfgException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IInvalidDateTimeSettings>();
                var value = settings.InvalidDateTime; // This should throw when parsing
            });

            Assert.That(ex.Message, Does.Contain("InvalidDateTime"), "Exception message should contain the property name 'InvalidDateTime'");
        }

        #endregion

        #region Valid Conversion Tests

        [Test]
        [Description("Verifies that valid type conversions succeed without errors")]
        public void ValidTypeConversion_Succeeds()
        {
            // Act
            var settings = MyAppCfg.Get<IValidTypeSettings>();

            // Assert
            Assert.AreEqual(42, settings.ValidInt, "Valid integer should be parsed as 42");
            Assert.AreEqual(true, settings.ValidBool, "Valid boolean should be parsed as true");
            Assert.AreEqual(3.14m, settings.ValidDecimal, "Valid decimal should be parsed as 3.14");
        }

        #endregion

        #region List Edge Case Tests

        [Test]
        [Description("Verifies that empty string value creates a list with one empty element")]
        public void EmptyString_ParsedAsSingleEmptyElement()
        {
            // Act
            var settings = MyAppCfg.Get<IListSettings>();

            // Assert
            Assert.IsNotNull(settings.EmptyList, "Empty list should not be null");
            Assert.AreEqual(1, settings.EmptyList.Count, "Empty string with separator should result in list with one element");
            Assert.AreEqual("", settings.EmptyList[0], "The single element should be an empty string");
        }

        #endregion

        #region Default Value Tests

        [Test]
        [Description("Verifies that default values work correctly for different types (int, string, bool)")]
        public void DefaultValue_WithDifferentTypes_WorksCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<IDefaultValueSettings>();

            // Assert
            Assert.AreEqual(99, settings.IntWithDefault, "Integer with default should be 99");
            Assert.AreEqual("default-string", settings.StringWithDefault, "String with default should be 'default-string'");
            Assert.AreEqual(true, settings.BoolWithDefault, "Boolean with default should be true");
        }

        #endregion

        // Test interfaces
        public interface IMissingValueSettings
        {
            [Option(Alias = "NonExistentKey")]
            string MissingString { get; }

            [Option(Alias = "NonExistentKey2", DefaultValue = "default-value")]
            string MissingStringWithDefault { get; }
        }

        public interface IInvalidIntSettings
        {
            [Option(Alias = "InvalidIntValue")]
            int InvalidInt { get; }
        }

        public interface IInvalidGuidSettings
        {
            [Option(Alias = "InvalidGuidValue")]
            Guid InvalidGuid { get; }
        }

        public interface IInvalidDateTimeSettings
        {
            [Option(Alias = "InvalidDateValue")]
            DateTime InvalidDateTime { get; }
        }

        public interface IValidTypeSettings
        {
            [Option(Alias = "ValidIntValue")]
            int ValidInt { get; }

            [Option(Alias = "ValidBoolValue")]
            bool ValidBool { get; }

            [Option(Alias = "ValidDecimalValue")]
            decimal ValidDecimal { get; }
        }

        public interface IListSettings
        {
            [Option(Alias = "EmptyListValue", Separator = ";")]
            List<string> EmptyList { get; }
        }

        public interface IDefaultValueSettings
        {
            [Option(Alias = "MissingInt", DefaultValue = 99)]
            int IntWithDefault { get; }

            [Option(Alias = "MissingString", DefaultValue = "default-string")]
            string StringWithDefault { get; }

            [Option(Alias = "MissingBool", DefaultValue = true)]
            bool BoolWithDefault { get; }
        }
    }
}
