/****** Object:  Table [dbo].[GlobalSettings]    Script Date: 4/20/2019 10:46:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GlobalSettings]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[GlobalSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TenantID] [nvarchar](50) NULL,
	[Name] [nvarchar](200) NULL,
	[Value] [ntext] NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_GlobalSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Settings]    Script Date: 4/20/2019 10:46:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Settings]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Settings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [nvarchar](50) NULL,
	[Name] [nvarchar](100) NULL,
	[Value] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET IDENTITY_INSERT [dbo].[GlobalSettings] ON 
GO
INSERT [dbo].[GlobalSettings] ([Id], [TenantID], [Name], [Value], [Description]) VALUES (1, NULL, N'Author', N'Jin', NULL)
GO
INSERT [dbo].[GlobalSettings] ([Id], [TenantID], [Name], [Value], [Description]) VALUES (3, N'I am a tenant', N'Author', N'Hung Vo <it.minhhung@gmail.com>', NULL)
GO
SET IDENTITY_INSERT [dbo].[GlobalSettings] OFF
GO
SET IDENTITY_INSERT [dbo].[Settings] ON 
GO
INSERT [dbo].[Settings] ([Id], [TenantId], [Name], [Value], [Description]) VALUES (1, NULL, N'PartnerKey', N'a3755435-0827-4e87-8d2a-c9126ca1064c
', NULL)
GO
INSERT [dbo].[Settings] ([Id], [TenantId], [Name], [Value], [Description]) VALUES (2, N'I am a tenant', N'PartnerKey', N'd323a1df-612d-42c8-a83f-e0ea333d7a22
', NULL)
GO
SET IDENTITY_INSERT [dbo].[Settings] OFF
GO
/****** Object:  StoredProcedure [dbo].[AppCfgGetSetting]    Script Date: 4/20/2019 10:46:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppCfgGetSetting]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[AppCfgGetSetting] AS' 
END
GO
ALTER PROCEDURE [dbo].[AppCfgGetSetting]
	@appcfg_tenant_name NVARCHAR(200),
	@appcfg_setting_name NVARCHAR(200)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
	IF(@appcfg_tenant_name IS NOT NULL)
	BEGIN 
		SELECT TOP (1) s.[Value] FROM dbo.Settings AS s WHERE s.TenantId = @appcfg_tenant_name AND s.Name = @appcfg_setting_name ORDER BY s.Id
	END
	ELSE
	BEGIN
		SELECT TOP (1) s.[Value] FROM dbo.Settings AS s WHERE s.TenantId IS NULL AND s.Name = @appcfg_setting_name ORDER BY s.Id
	END
END
GO