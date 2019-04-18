using AppCfg;
using AppCfg.SettingStore;
using System.Configuration;

namespace AppCfgDemoMssql
{
    /// <summary>
    /// Setting wrapper class, this also help us cache setting
    /// In case you want to refresh setting value every time you get setting (for example if you get setting from database)
    /// then you have to call MyAppCfg.Get<Your_Setting_Here>() directly
    /// </summary>
    public class MySettings
    {
        public const string StoreKey_One = "first-key-demo > dbo.global table";
        public const string StoreKey_Two = "second-key-demo > dbo.setting table";

        public static void Init()
        {
            // 1. Make sure that your database has at least 4 columns: TenantId/SettingGroup/Name/Value

            // 2.if you use MSSQL, AppCfg.Net need to know how to get your value
            // Either using command text or using stored procedure, it's up to you

            // 3. If you use CommandText then define a query text to access setting's value
            // NOTE: If you use command text, be careful with TenantId NULL and EMPTY (you can not define query like: [TenantId] = NULL)
            // I would recommend using Stored Procedure instead of CommandText
            MyAppCfg.SettingStores.RegisterMsSqlDatabaseStore(StoreKey_One,
                new MsSqlSettingStoreConfig
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["myConn"].ConnectionString,
                    QueryCmd = "SELECT TOP 1 [Value] FROM [GlobalSettings] WHERE [TenantId] = '{0}' AND [Name] = '{1}'", // {0} must be tenantId, {1} must be setting-name
                    QueryCmdType = QueryCmdType.Text,
                });

            // 4. If you use Stored Proc, make sure that your stored proc supports 2 parameters: 
            // - @appcfg_tenant_name,  
            // - @appcfg_setting_name
            // check stored 'dbo.AppCfgGetSetting' in 'test_database.sql' script for more information
            MyAppCfg.SettingStores.RegisterMsSqlDatabaseStore(StoreKey_Two,
                new MsSqlSettingStoreConfig
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["myConn"].ConnectionString,
                    QueryCmd = "dbo.AppCfgGetSetting",
                    QueryCmdType = QueryCmdType.StoreProcedure,
                });

            // inital settings
            BaseSettings = MyAppCfg.Get<ISetting>();
        }

        public static ISetting BaseSettings;
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
