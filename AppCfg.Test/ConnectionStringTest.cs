using NUnit.Framework;
using System.Data.SqlClient;

namespace AppCfg.Test
{
    [TestFixture]
    [Description("Tests for SQL connection string parsing from App.config")]
    public class ConnectionStringTest
    {
        private IConnectionStringSetting _settings;

        [SetUp]
        public void Setup()
        {
            // Reset configuration state to ensure clean test
            // This prevents Configure() from previous tests affecting this one
            _settings = MyAppCfg.Get<IConnectionStringSetting>();
        }

        [Test]
        [Description("Verifies that full connection string is loaded and parsed from App.config")]
        public void ConnectionString_FromAppConfig_ParsedCorrectly()
        {
            Assert.IsNotNull(_settings.IamConn, "Connection string should not be null");
            Assert.AreEqual(
                @"Data Source=(local);Initial Catalog=Microsoft;User ID=u4erN@me;Password=p@55w0rd;Connect Timeout=180",
                _settings.IamConn.ConnectionString,
                "Full connection string should match expected value");
        }

        [Test]
        [Description("Verifies that InitialCatalog property is correctly extracted from connection string")]
        public void ConnectionString_InitialCatalog_ParsedCorrectly()
        {
            Assert.AreEqual("Microsoft", _settings.IamConn.InitialCatalog, "InitialCatalog should be 'Microsoft'");
        }

        [Test]
        [Description("Verifies that ConnectTimeout property is correctly extracted from connection string")]
        public void ConnectionString_ConnectTimeout_ParsedCorrectly()
        {
            Assert.AreEqual(180, _settings.IamConn.ConnectTimeout, "ConnectTimeout should be 180 seconds");
        }

        [Test]
        [Description("Verifies that UserID property is correctly extracted from connection string")]
        public void ConnectionString_UserID_ParsedCorrectly()
        {
            Assert.AreEqual("u4erN@me", _settings.IamConn.UserID, "UserID should be 'u4erN@me'");
        }

        [Test]
        [Description("Verifies that Password property is correctly extracted from connection string")]
        public void ConnectionString_Password_ParsedCorrectly()
        {
            Assert.AreEqual("p@55w0rd", _settings.IamConn.Password, "Password should be 'p@55w0rd'");
        }

        [Test]
        [Description("Verifies that DataSource property is correctly extracted from connection string")]
        public void ConnectionString_DataSource_ParsedCorrectly()
        {
            Assert.AreEqual("(local)", _settings.IamConn.DataSource, "DataSource should be '(local)'");
        }

        [Test]
        [Description("Verifies that connection string parsing works correctly after Configure() is called")]
        public void ConnectionString_WithConfigure_StillWorks()
        {
            // Arrange - Configure with priority-based loading
            MyAppCfg.Configure(envVarPrefix: "TEST__", userSecretsId: null);

            // Act
            var settings = MyAppCfg.Get<IConnectionStringSetting>();

            // Assert - Should still fall back to App.config
            Assert.IsNotNull(settings.IamConn, "Connection string should not be null after Configure()");
            Assert.AreEqual("Microsoft", settings.IamConn.InitialCatalog, "InitialCatalog should still be 'Microsoft' after Configure()");
        }

        [Test]
        [Description("Verifies that with Configure(), a SqlConnectionStringBuilder key present in both appSettings and connectionStrings is read from connectionStrings")]
        public void ConnectionString_WithConfigure_KeyAlsoInAppSettings_PrefersConnectionStrings()
        {
            // Arrange
            MyAppCfg.Configure(envVarPrefix: "TEST__", userSecretsId: null);

            // Act
            var settings = MyAppCfg.Get<IDuplicateKeyConnectionSetting>();

            // Assert
            Assert.AreEqual("DupDb", settings.DupConn.InitialCatalog, "connectionStrings entry should win for SqlConnectionStringBuilder");
        }

        public interface IDuplicateKeyConnectionSetting
        {
            [Option(Alias = "dupConn")]
            SqlConnectionStringBuilder DupConn { get; }
        }

        // Test interface
        public interface IConnectionStringSetting
        {
            [Option(Alias = "myConn")]
            SqlConnectionStringBuilder IamConn { get; }
        }
    }
}
