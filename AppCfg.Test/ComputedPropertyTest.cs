using AppCfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AppCfg.Test
{
    [TestFixture]
    public class ComputedPropertyTest
    {
        [Test]
        public void ComputedProperty_SimpleString_ReturnsComputedValue()
        {
            // Act
            var settings = MyAppCfg.Get<IDatabaseSettings>();

            // Assert
            Assert.AreEqual("localhost:5432", settings.HostAndPort);
        }

        [Test]
        public void ComputedProperty_ComplexLogic_ReturnsComputedValue()
        {
            // Act
            var settings = MyAppCfg.Get<IDatabaseSettings>();

            // Assert
            Assert.AreEqual("Server=localhost;Port=5432;Database=testdb;", settings.ConnectionString);
        }

        [Test]
        public void ComputedProperty_AccessesMultipleProperties_WorksCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<IDatabaseSettings>();

            // Assert
            // FullPath should be computed from Host, Port, and Database
            Assert.AreEqual("localhost:5432/testdb", settings.FullPath);
        }

        [Test]
        public void ComputedProperty_WithDefaultValue_UsesDefault()
        {
            // Act
            var settings = MyAppCfg.Get<ISettingsWithDefaults>();

            // Assert
            // Timeout has default, so computed should use it
            Assert.AreEqual("Timeout is: 30", settings.TimeoutMessage);
        }

        [Test]
        public void ComputedProperty_DifferentTypes_WorksCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<IApiSettings>();

            // Assert
            Assert.AreEqual("https://api.example.com/v2", settings.FullApiUrl);
            Assert.AreEqual(10, settings.MaxRetriesDoubled);
        }

        [Test]
        public void ComputedProperty_ReturnsNull_AllowsNull()
        {
            // Act
            var settings = MyAppCfg.Get<IApiSettings>();

            // Assert
            Assert.IsNull(settings.OptionalValue);
        }

        [Test]
        public void ComputedProperty_ReturnsList_WorksCorrectly()
        {
            // Act
            var settings = MyAppCfg.Get<IApiSettings>();

            // Assert
            Assert.IsNotNull(settings.AllEndpoints);
            Assert.AreEqual(2, settings.AllEndpoints.Count);
            Assert.Contains("https://api.example.com/v2/users", settings.AllEndpoints);
            Assert.Contains("https://api.example.com/v2/products", settings.AllEndpoints);
        }

        [Test]
        public void ComputedProperty_InvalidMethodName_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IInvalidComputedSettings>();
                var _ = settings.BadProperty;
            });

            Assert.That(ex.Message, Does.Contain("Computed method not found"));
            Assert.That(ex.Message, Does.Contain("NonExistentMethod"));
        }

        [Test]
        public void ComputedProperty_WrongReturnType_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IWrongReturnTypeSettings>();
                var _ = settings.WrongType;
            });

            Assert.That(ex.Message, Does.Contain("return type mismatch"));
        }

        [Test]
        public void ComputedProperty_ThrowsException_WrapsInAppCfgException()
        {
            // Act & Assert
            var ex = Assert.Throws<AppCfgException>(() =>
            {
                var settings = MyAppCfg.Get<IThrowingComputedSettings>();
                var _ = settings.ThrowingProperty;
            });

            Assert.That(ex.Message, Does.Contain("Error computing property"));
            Assert.That(ex.Message, Does.Contain("ThrowingProperty"));
        }

        [SetUp]
        public void Setup()
        {
            // Values are in App.config
        }

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
