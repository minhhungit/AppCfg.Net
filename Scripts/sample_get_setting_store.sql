IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppCfgGetSetting]') AND type IN (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[AppCfgGetSetting] AS' 
END
GO

ALTER PROCEDURE [dbo].[AppCfgGetSetting]
	@appcfg_setting_name NVARCHAR(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
	SELECT s.[Value] FROM dbo.Settings AS s WHERE s.Name = @appcfg_setting_name
END

GO
