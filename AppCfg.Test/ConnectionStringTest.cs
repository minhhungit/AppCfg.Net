using NUnit.Framework;
using System.Data.SqlClient;

namespace AppCfg.Test
{
    [TestFixture]
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
        public void ConnectionString_FromAppConfig_ParsedCorrectly()
        {
            // Assert
            Assert.IsNotNull(_settings.IamConn, "Connection string should not be null");
            Assert.AreEqual(@"Data Source=(local);Initial Catalog=Microsoft;User ID=u4erN@me;Password=p@55w0rd;Connect Timeout=180", _settings.IamConn.ConnectionString);
        }

        [Test]
        public void ConnectionString_InitialCatalog_ParsedCorrectly()
        {
            // Assert
            Assert.AreEqual("Microsoft", _settings.IamConn.InitialCatalog);
        }

        [Test]
        public void ConnectionString_ConnectTimeout_ParsedCorrectly()
        {
            // Assert
            Assert.AreEqual(180, _settings.IamConn.ConnectTimeout);
        }

        [Test]
        public void ConnectionString_UserID_ParsedCorrectly()
        {
            // Assert
            Assert.AreEqual("u4erN@me", _settings.IamConn.UserID);
        }

        [Test]
        public void ConnectionString_Password_ParsedCorrectly()
        {
            // Assert
            Assert.AreEqual("p@55w0rd", _settings.IamConn.Password);
        }

        [Test]
        public void ConnectionString_DataSource_ParsedCorrectly()
        {
            // Assert
            Assert.AreEqual("(local)", _settings.IamConn.DataSource);
        }

        [Test]
        public void ConnectionString_WithConfigure_StillWorks()
        {
            // Arrange - Configure with priority-based loading
            MyAppCfg.Configure(envVarPrefix: "TEST__", userSecretsId: null);

            // Act
            var settings = MyAppCfg.Get<IConnectionStringSetting>();

            // Assert - Should still fall back to App.config
            Assert.IsNotNull(settings.IamConn);
            Assert.AreEqual("Microsoft", settings.IamConn.InitialCatalog);
        }

        // Test interface
        public interface IConnectionStringSetting
        {
            [Option(Alias = "myConn")]
            SqlConnectionStringBuilder IamConn { get; }
        }
    }
}
