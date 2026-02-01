using AppCfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Tests for Computed property attribute functionality")]
    public class ComputedPropertyTest
    {
        [SetUp]
        public void Setup()
        {
            // Values are in App.config
        }

        #region Basic Computed Property Tests

        [Test]
        [Description("Verifies that computed property returns a simple string combining host and port")]
        public void ComputedProperty_SimpleString_ReturnsComputedValue()
        {
            // Act
            var settings = MyAppCfg.Get<IDatabaseSettings>();

            // Assert
            Assert.AreEqual("localhost:5432", settings.HostAndPort, "Computed HostAndPort should combine Host and Port");
        }

        [Test]
        [Description("Verifies that computed property can build a complex connection string")]
        public void ComputedProperty_ComplexLogic_ReturnsComputedValue()
        {
            // Act
            var settings = MyAppCfg.Get<IDatabaseSettings>();

            // Assert
            Assert.AreEqual("Server=localhost;Port=5432;Database=testdb;", settings.ConnectionString, "Computed ConnectionString should build correct format");
        }

        [Test]
        [Description("Verifies that computed property can access multiple properties from the settings interface")]
        public void ComputedProperty_AccessesMultipleProperties_WorksCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<IDatabaseSettings>();

            // Assert
            // FullPath should be computed from Host, Port, and Database
            Assert.AreEqual("localhost:5432/testdb", settings.FullPath, "Computed FullPath should combine Host, Port, and Database");
        }

        [Test]
        [Description("Verifies that computed property works correctly with default values")]
        public void ComputedProperty_WithDefaultValue_UsesDefault()
        {
            // Act
            var settings = MyAppCfg.Get<ISettingsWithDefaults>();

            // Assert
            // Timeout has default, so computed should use it
            Assert.AreEqual("Timeout is: 30", settings.TimeoutMessage, "Computed should use default value when setting is missing");
        }

        #endregion

        #region Different Return Type Tests

        [Test]
        [Description("Verifies that computed properties work with different return types (string and int)")]
        public void ComputedProperty_DifferentTypes_WorksCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<IApiSettings>();

            // Assert
            Assert.AreEqual("https://api.example.com/v2", settings.FullApiUrl, "Computed string URL should be built correctly");
            Assert.AreEqual(10, settings.MaxRetriesDoubled, "Computed int should double the MaxRetries value");
        }

        [Test]
        [Description("Verifies that computed property can return null")]
        public void ComputedProperty_ReturnsNull_AllowsNull()
        {
            // Act
            var settings = MyAppCfg.Get<IApiSettings>();

            // Assert
            Assert.IsNull(settings.OptionalValue, "Computed property should be able to return null");
        }

        [Test]
        [Description("Verifies that computed property can return a List")]
        public void ComputedProperty_ReturnsList_WorksCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<IApiSettings>();

            // Assert
            Assert.IsNotNull(settings.AllEndpoints, "Computed list should not be null");
            Assert.AreEqual(2, settings.AllEndpoints.Count, "Computed list should contain 2 endpoints");
            Assert.Contains("https://api.example.com/v2/users", settings.AllEndpoints, "List should contain users endpoint");
            Assert.Contains("https://api.example.com/v2/products", settings.AllEndpoints, "List should contain products endpoint");
        }

        #endregion

        #region Error Handling Tests

        [Test]
        [Description("Verifies that invalid method name in Computed attribute throws AppCfgException")]
        public void ComputedProperty_InvalidMethodName_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IInvalidComputedSettings>();
                var _ = settings.BadProperty;
            });

            Assert.That(ex.Message, Does.Contain("Computed method not found"), "Exception should indicate method not found");
            Assert.That(ex.Message, Does.Contain("NonExistentMethod"), "Exception should include the invalid method name");
        }

        [Test]
        [Description("Verifies that wrong return type in compute method throws AppCfgException")]
        public void ComputedProperty_WrongReturnType_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IWrongReturnTypeSettings>();
                var _ = settings.WrongType;
            });

            Assert.That(ex.Message, Does.Contain("return type mismatch"), "Exception should indicate return type mismatch");
        }

        [Test]
        [Description("Verifies that exception thrown in compute method is wrapped in AppCfgException")]
        public void ComputedProperty_ThrowsException_WrapsInAppCfgException()
        {
            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IThrowingComputedSettings>();
                var _ = settings.ThrowingProperty;
            });

            Assert.That(ex.Message, Does.Contain("Error computing property"), "Exception should indicate computation error");
            Assert.That(ex.Message, Does.Contain("ThrowingProperty"), "Exception should include the property name");
        }

        #endregion

        // Test interfaces
        public interface IDatabaseSettings
        {
            [Option(Alias = "DB:Host")]
            string Host { get; }

            [Option(Alias = "DB:Port")]
            int Port { get; }

            [Option(Alias = "DB:Database")]
            string Database { get; }

            [Computed(typeof(DatabaseHelpers), nameof(DatabaseHelpers.GetHostAndPort))]
            string HostAndPort { get; }

            [Computed(typeof(DatabaseHelpers), nameof(DatabaseHelpers.BuildConnectionString))]
            string ConnectionString { get; }

            [Computed(typeof(DatabaseHelpers), nameof(DatabaseHelpers.GetFullPath))]
            string FullPath { get; }
        }

        public interface ISettingsWithDefaults
        {
            [Option(Alias = "TimeoutSeconds", DefaultValue = 30)]
            int Timeout { get; }

            [Computed(typeof(ComputeHelpers), nameof(ComputeHelpers.GetTimeoutMessage))]
            string TimeoutMessage { get; }
        }

        public interface IApiSettings
        {
            [Option(Alias = "Api:BaseUrl")]
            string BaseUrl { get; }

            [Option(Alias = "Api:Version")]
            string Version { get; }

            [Option(Alias = "Api:MaxRetries")]
            int MaxRetries { get; }

            [Computed(typeof(ApiHelpers), nameof(ApiHelpers.BuildFullUrl))]
            string FullApiUrl { get; }

            [Computed(typeof(ApiHelpers), nameof(ApiHelpers.DoubleMaxRetries))]
            int MaxRetriesDoubled { get; }

            [Computed(typeof(ApiHelpers), nameof(ApiHelpers.GetOptionalValue))]
            string OptionalValue { get; }

            [Computed(typeof(ApiHelpers), nameof(ApiHelpers.GetAllEndpoints))]
            List<string> AllEndpoints { get; }
        }

        public interface IInvalidComputedSettings
        {
            [Computed(typeof(InvalidHelpers), "NonExistentMethod")]
            string BadProperty { get; }
        }

        public interface IWrongReturnTypeSettings
        {
            [Computed(typeof(InvalidHelpers), nameof(InvalidHelpers.ReturnsWrongType))]
            string WrongType { get; }
        }

        public interface IThrowingComputedSettings
        {
            [Computed(typeof(InvalidHelpers), nameof(InvalidHelpers.ThrowsException))]
            string ThrowingProperty { get; }
        }
    }

    // Helper classes
    public static class DatabaseHelpers
    {
        public static string GetHostAndPort(ComputedPropertyTest.IDatabaseSettings settings)
        {
            return $"{settings.Host}:{settings.Port}";
        }

        public static string BuildConnectionString(ComputedPropertyTest.IDatabaseSettings settings)
        {
            return $"Server={settings.Host};Port={settings.Port};Database={settings.Database};";
        }

        public static string GetFullPath(ComputedPropertyTest.IDatabaseSettings settings)
        {
            return $"{settings.Host}:{settings.Port}/{settings.Database}";
        }
    }

    public static class ComputeHelpers
    {
        public static string GetTimeoutMessage(ComputedPropertyTest.ISettingsWithDefaults settings)
        {
            return $"Timeout is: {settings.Timeout}";
        }
    }

    public static class ApiHelpers
    {
        public static string BuildFullUrl(ComputedPropertyTest.IApiSettings settings)
        {
            return $"{settings.BaseUrl}/{settings.Version}";
        }

        public static int DoubleMaxRetries(ComputedPropertyTest.IApiSettings settings)
        {
            return settings.MaxRetries * 2;
        }

        public static string GetOptionalValue(ComputedPropertyTest.IApiSettings settings)
        {
            return null; // Intentionally return null
        }

        public static List<string> GetAllEndpoints(ComputedPropertyTest.IApiSettings settings)
        {
            var baseUrl = $"{settings.BaseUrl}/{settings.Version}";
            return new List<string>
            {
                $"{baseUrl}/users",
                $"{baseUrl}/products"
            };
        }
    }

    public static class InvalidHelpers
    {
        public static int ReturnsWrongType(ComputedPropertyTest.IWrongReturnTypeSettings settings)
        {
            return 42; // Returns int but property expects string
        }

        public static string ThrowsException(ComputedPropertyTest.IThrowingComputedSettings settings)
        {
            throw new InvalidOperationException("Intentional test exception");
        }
    }
}
