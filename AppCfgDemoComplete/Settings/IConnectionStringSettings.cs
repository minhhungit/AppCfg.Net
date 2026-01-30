using AppCfg;
using System.Data.SqlClient;

namespace AppCfgDemoComplete.Settings
{
    public interface IConnectionStringSettings
    {
        [Option(Alias = "myConnFirst")]
        SqlConnectionStringBuilder First { get; }

        [Option(Alias = "myConnSecond")]
        SqlConnectionStringBuilder Second { get; }
    }
}
