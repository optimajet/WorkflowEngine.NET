IF ( EXISTS (select * from sys.triggers where name = 'tWorkflowProcessTransitionHistoryInsert'))
BEGIN
	DROP TRIGGER  tWorkflowProcessTransitionHistoryInsert
	PRINT 'DROP TRIGGER  tWorkflowProcessTransitionHistoryInsert'
END

GO

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessScheme' AND [COLUMN_NAME] = N'ProcessName')
BEGIN
	EXEC sp_rename 'WorkflowProcessScheme.ProcessName', 'SchemeCode', 'COLUMN';
	PRINT 'RENAME WorkflowProcessScheme.ProcessName -> SchemeCode'
END

GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessTimer')
BEGIN
CREATE TABLE [dbo].[WorkflowProcessTimer](
	[Id] [uniqueidentifier] NOT NULL,
	[ProcessId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[NextExecutionDateTime] [datetime] NOT NULL,
	[Ignore] [bit] NOT NULL,
 CONSTRAINT [PK_WorkflowProcessTimer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

PRINT 'WorkflowProcessTimer CREATE TABLE'

END

GO

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory' AND [COLUMN_NAME] = N'ExecutorIdentityId' AND [DATA_TYPE] = N'uniqueidentifier')
BEGIN
	ALTER TABLE WorkflowProcessTransitionHistory ALTER COLUMN ExecutorIdentityId nvarchar(max) NOT NULL
	PRINT 'RENAME WorkflowProcessTransitionHistory.ExecutorIdentityId type uniqueidentifier -> nvarchar(max)'
END

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory' AND [COLUMN_NAME] = N'ActorIdentityId' AND [DATA_TYPE] = N'uniqueidentifier')
BEGIN
	ALTER TABLE WorkflowProcessTransitionHistory ALTER COLUMN ActorIdentityId nvarchar(max) NOT NULL
	PRINT 'RENAME WorkflowProcessTransitionHistory.ActorIdentityId type uniqueidentifier -> nvarchar(max)'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory' AND [COLUMN_NAME] = N'TriggerName')
BEGIN
	ALTER TABLE WorkflowProcessTransitionHistory ADD TriggerName nvarchar(max) NULL
	PRINT 'ADD WorkflowProcessTransitionHistory.TriggerName type nvarchar(max)'
END
