using AppCfg;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace AppCfgDemoMssql
{
    public interface IMssqlSetting
    {
        [Option(Alias = "Author", StoreType = SettingStoreType.Custom, StoreIdentity = MySettings.StoreKey_MSSQL_With_CommandText)]
        string ASettingFromDb_Text { get; }

        [Option(Alias = "NO-SETTING", DefaultValue = "I am default value", StoreType = SettingStoreType.Custom, StoreIdentity = MySettings.StoreKey_MSSQL_With_CommandText)]
        string DemoDefault_Text { get; }

        [Option(Alias = "PartnerKey", StoreType = SettingStoreType.Custom, StoreIdentity = MySettings.StoreKey_MSSQL_With_StoredProc)]
        Guid ASettingFromDb_Stored { get; }
    }

    /// <summary>
    /// Setting wrapper class, this also help us cache setting
    /// In case you want to refresh setting value every time you get setting (for example if you get setting from database)
    /// then you have to call MyAppCfg.Get<Your_Setting_Here>() directly
    /// </summary>
    public class MySettings
    {
        public const string StoreKey_MSSQL_With_CommandText = "MSSQL > first-key-demo > dbo.global table";
        public const string StoreKey_MSSQL_With_StoredProc = "MSSQL > second-key-demo > dbo.setting table";

        public static void Init()
        {
            MyAppCfg.SettingStores.RegisterCustomStore(StoreKey_MSSQL_With_CommandText, opt =>
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["myConn"].ConnectionString;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        var sqlText = string.Empty;
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
                            command.CommandType = System.Data.CommandType.Text;

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


            MyAppCfg.SettingStores.RegisterCustomStore(StoreKey_MSSQL_With_StoredProc, opt =>
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["myConn"].ConnectionString;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand("dbo.AppCfgGetSetting", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;

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
            
            // inital settings
            BaseSettings = MyAppCfg.Get<IMssqlSetting>();
            BaseSettingsWithTenant = MyAppCfg.Get<IMssqlSetting>("I am a tenant");
        }

        public static IMssqlSetting BaseSettings;
        public static IMssqlSetting BaseSettingsWithTenant;
    }
}

/* 
CREATE PROCEDURE [dbo].[AppCfgGetSetting]
	@appcfg_tenant_name NVARCHAR(200),
	@appcfg_setting_name NVARCHAR(200)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
	IF(@appcfg_tenant_name IS NOT NULL)
	BEGIN 
		SELECT TOP (1) s.[Value] FROM dbo.Settings AS s WHERE s.TenantId = @appcfg_tenant_name	AND s.Name = @appcfg_setting_name ORDER BY s.Id
	END
	ELSE
	BEGIN
		SELECT TOP (1) s.[Value] FROM dbo.Settings AS s WHERE s.TenantId IS NULL				AND s.Name = @appcfg_setting_name ORDER BY s.Id
	END
END
*/
