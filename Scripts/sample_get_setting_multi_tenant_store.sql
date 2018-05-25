IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppCfgGetSetting]') AND type IN (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[AppCfgGetSetting] AS' 
END
GO

ALTER PROCEDURE [dbo].[AppCfgGetSetting]
	@appcfg_setting_name NVARCHAR(100),
	@appcfg_tenant_name NVARCHAR(100) -- make sure that your setting table has column <TenantName> or something like that
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
	IF(@appcfg_tenant_name IS NOT NULL)
	BEGIN 
		SELECT s.[Value] FROM dbo.Settings AS s WHERE s.Name = @appcfg_setting_name AND s.TenantName = @appcfg_tenant_name
	END
	ELSE
	BEGIN
		SELECT s.[Value] FROM dbo.Settings AS s WHERE s.Name = @appcfg_setting_name
	END
END

GO


