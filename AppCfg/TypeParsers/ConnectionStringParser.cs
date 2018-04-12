using System.Data.SqlClient;

namespace AppCfg.TypeParsers
{
    public class ConnectionStringParser : ITypeParser<SqlConnectionStringBuilder>
    {
        public SqlConnectionStringBuilder Parse(string rawValue, ITypeParserOptions options)
        {
            return new SqlConnectionStringBuilder(rawValue);
        }
    }
}
