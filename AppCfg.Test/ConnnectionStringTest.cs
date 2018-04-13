using NUnit.Framework;
using System;
using System.Data.SqlClient;

namespace AppCfg.Test
{
    public interface IConnectionStringSetting
    {
        [Option(Alias = "myConn")]
        SqlConnectionStringBuilder IamConn { get; }
    }

    [TestFixture]
    public class ConnnectionStringTest
    {
        IConnectionStringSetting settings = MyAppCfg.Get<IConnectionStringSetting>();

        [Test]
        public void Test_ConnnectionString()
        {
            Assert.AreEqual(settings.IamConn.ConnectionString, @"Data Source=(local);Initial Catalog=Microsoft;User ID=u4erN@me;Password=p@55w0rd;Connect Timeout=180");
            Assert.AreEqual(settings.IamConn.InitialCatalog, "Microsoft");
            Assert.AreEqual(settings.IamConn.ConnectTimeout, 180);
            Assert.AreEqual(settings.IamConn.UserID, "u4erN@me");
            Assert.AreEqual(settings.IamConn.Password, "p@55w0rd");
        }
    }
}
