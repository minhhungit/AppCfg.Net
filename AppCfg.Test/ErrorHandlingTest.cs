using AppCfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    [TestFixture]
    public class ErrorHandlingTest
    {
        [Test]
        public void MissingRequiredSetting_WithoutDefault_ReturnsNull()
        {
            // Act
            var settings = MyAppCfg.Get<IMissingValueSettings>();

            // Assert
            Assert.IsNull(settings.MissingString, "Missing string without default should be null");
        }

        [Test]
        public void MissingRequiredSetting_WithDefault_ReturnsDefault()
        {
            // Act
            var settings = MyAppCfg.Get<IMissingValueSettings>();

            // Assert
            Assert.AreEqual("default-value", settings.MissingStringWithDefault);
        }

        [Test]
        public void InvalidIntTypeConversion_ThrowsAppCfgException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IInvalidIntSettings>();
                var value = settings.InvalidInt; // This should throw when parsing
            });

            Assert.That(ex.Message, Does.Contain("InvalidInt"));
        }

        [Test]
        public void InvalidGuidFormat_ThrowsAppCfgException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IInvalidGuidSettings>();
                var value = settings.InvalidGuid; // This should throw when parsing
            });

            Assert.That(ex.Message, Does.Contain("InvalidGuid"));
        }

        [Test]
        public void InvalidDateTimeFormat_ThrowsAppCfgException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IInvalidDateTimeSettings>();
                var value = settings.InvalidDateTime; // This should throw when parsing
            });

            Assert.That(ex.Message, Does.Contain("InvalidDateTime"));
        }

        [Test]
        public void ValidTypeConversion_Succeeds()
        {
            // Act
            var settings = MyAppCfg.Get<IValidTypeSettings>();

            // Assert
            Assert.AreEqual(42, settings.ValidInt);
            Assert.AreEqual(true, settings.ValidBool);
            Assert.AreEqual(3.14m, settings.ValidDecimal);
        }

        [Test]
        public void EmptyString_ParsedAsSingleEmptyElement()
        {
            // Act
            var settings = MyAppCfg.Get<IListSettings>();

            // Assert
            Assert.IsNotNull(settings.EmptyList);
            // Empty string with separator results in one empty element
            Assert.AreEqual(1, settings.EmptyList.Count);
            Assert.AreEqual("", settings.EmptyList[0]);
        }

        [Test]
        public void DefaultValue_WithDifferentTypes_WorksCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<IDefaultValueSettings>();

            // Assert
            Assert.AreEqual(99, settings.IntWithDefault);
            Assert.AreEqual("default-string", settings.StringWithDefault);
            Assert.AreEqual(true, settings.BoolWithDefault);
        }

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

        [SetUp]
        public void Setup()
        {
            // Note: Values are configured in App.config
            // ConfigurationManager.AppSettings is read-only at runtime
        }
    }
}
