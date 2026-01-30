using AppCfg;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AppCfgDemoComplete.CustomStores
{
    /// <summary>
    /// Demonstrates custom MSSQL database store registration.
    /// Shows two approaches: CommandText and StoredProcedure.
    /// This is a real implementation that connects to SQL Server.
    /// </summary>
    public static class MssqlStore
    {
        public const string StoreKey_CommandText = "MSSQL > CommandText > GlobalSettings";
        public const string StoreKey_StoredProc = "MSSQL > StoredProc > Settings";

        /// <summary>
        /// Register custom MSSQL stores.
        /// Demonstrates the REGISTRATION API for custom stores.
        /// </summary>
        public static void Register()
        {
            // Store #1: Using CommandText (SQL query)
            MyAppCfg.SettingStores.RegisterStore(StoreKey_CommandText, opt =>
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DemoDatabase"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("DemoDatabase connection string not found in App.config");
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Build SQL query based on tenant
                    string sqlText;
                    if (string.IsNullOrWhiteSpace(opt.TenantKey))
                    {
                        sqlText = $"SELECT TOP 1 [Value] FROM [GlobalSettings] WHERE [TenantId] IS NULL AND [Name] = '{opt.SettingKey}'";
                    }
                    else
                    {
                        sqlText = $"SELECT TOP 1 [Value] FROM [GlobalSettings] WHERE [TenantId] = '{opt.TenantKey}' AND [Name] = '{opt.SettingKey}'";
                    }

                    using (SqlCommand command = new SqlCommand(sqlText, connection))
                    {
                        command.CommandType = CommandType.Text;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return reader.GetString(0);
                            }
                        }

                        return null;
                    }
                }
            });

            // Store #2: Using StoredProcedure
            MyAppCfg.SettingStores.RegisterStore(StoreKey_StoredProc, opt =>
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DemoDatabase"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("DemoDatabase connection string not found in App.config");
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("dbo.AppCfgGetSetting", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@appcfg_tenant_name", opt.TenantKey ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@appcfg_setting_name", opt.SettingKey));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return reader.GetString(0);
                            }
                        }

                        return null;
                    }
                }
            });
        }
    }
}

/* SQL Script for Database Setup:

-- Create GlobalSettings table
CREATE TABLE [dbo].[GlobalSettings] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TenantId] NVARCHAR(200) NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL
);

-- Create Settings table
CREATE TABLE [dbo].[Settings] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TenantId] NVARCHAR(200) NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL
);

-- Create stored procedure
CREATE PROCEDURE [dbo].[AppCfgGetSetting]
    @appcfg_tenant_name NVARCHAR(200),
    @appcfg_setting_name NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@appcfg_tenant_name IS NOT NULL)
    BEGIN
        SELECT TOP (1) s.[Value]
        FROM dbo.Settings AS s
        WHERE s.TenantId = @appcfg_tenant_name
          AND s.Name = @appcfg_setting_name
        ORDER BY s.Id
    END
    ELSE
    BEGIN
        SELECT TOP (1) s.[Value]
        FROM dbo.Settings AS s
        WHERE s.TenantId IS NULL
          AND s.Name = @appcfg_setting_name
        ORDER BY s.Id
    END
END

-- Insert sample data
INSERT INTO GlobalSettings (TenantId, Name, Value) VALUES
    (NULL, 'Author', 'John Doe'),
    ('tenant-1', 'Author', 'Jane Smith');

INSERT INTO Settings (TenantId, Name, Value) VALUES
    (NULL, 'PartnerKey', '12345678-1234-1234-1234-123456789012'),
    ('tenant-1', 'PartnerKey', '87654321-4321-4321-4321-210987654321');

*/
