IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowGlobalParameter')
BEGIN
CREATE TABLE [dbo].[WorkflowGlobalParameter](
	[Id] [uniqueidentifier] NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Value]  [nvarchar](max) NOT NULL
 CONSTRAINT [PK_WorkflowGlobalParameter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

PRINT 'WorkflowGlobalParameter CREATE TABLE'

END

GO

