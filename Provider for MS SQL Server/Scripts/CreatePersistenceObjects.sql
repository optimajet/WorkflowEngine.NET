/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MSSQL
Version: 2.2
File: CreatePersistenceObjects.sql

*/


BEGIN TRANSACTION

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessScheme')
BEGIN
	CREATE TABLE [WorkflowProcessScheme](
		[Id] [uniqueidentifier] NOT NULL,
		[Scheme] [ntext] NOT NULL,
		[DefiningParameters] [ntext] NOT NULL,
		[DefiningParametersHash] [nvarchar](1024) NOT NULL,
		[SchemeCode] [nvarchar](max) NOT NULL,
		[IsObsolete] [bit] NOT NULL DEFAULT (0),
		[RootSchemeCode] nvarchar (max) NULL,
		[RootSchemeId]  uniqueidentifier NULL,
		[AllowedActivities] nvarchar (max) NULL,
		[StartingTransition] nvarchar (max) NULL,		
	 CONSTRAINT [PK_WorkflowProcessScheme] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	PRINT 'WorkflowProcessScheme CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessInstance')
BEGIN
	CREATE TABLE [WorkflowProcessInstance](
		[Id] [uniqueidentifier] NOT NULL,
		[StateName] [nvarchar](max) NULL,
		[ActivityName] [nvarchar](max) NOT NULL,
		[SchemeId] [uniqueidentifier] NULL,
		[PreviousState] [nvarchar](max) NULL,
		[PreviousStateForDirect] [nvarchar](max) NULL,
		[PreviousStateForReverse] [nvarchar](max) NULL,
		[PreviousActivity] [nvarchar](max) NULL,
		[PreviousActivityForDirect] [nvarchar](max) NULL,
		[PreviousActivityForReverse] [nvarchar](max) NULL,
		[ParentProcessId] uniqueidentifier NULL,
		[RootProcessId] uniqueidentifier NOT NULL,
		[IsDeterminingParametersChanged] [bit] NOT NULL DEFAULT ((0)),

	 CONSTRAINT [PK_WorkflowProcessInstance_1] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

	PRINT 'WorkflowProcessInstance CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessInstancePersistence')
BEGIN
	CREATE TABLE [WorkflowProcessInstancePersistence](
		[Id] [uniqueidentifier] NOT NULL,
		[ProcessId] [uniqueidentifier] NOT NULL,
		[ParameterName] [nvarchar](max) NOT NULL,
		[Value] [ntext] NOT NULL,
	 CONSTRAINT [PK_WorkflowProcessInstancePersistence] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	PRINT 'WorkflowProcessInstancePersistence CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory')
BEGIN
	CREATE TABLE [WorkflowProcessTransitionHistory](
		[Id] [uniqueidentifier] NOT NULL,
		[ProcessId] [uniqueidentifier] NOT NULL,
		[ExecutorIdentityId] [nvarchar](max) NULL,
		[ActorIdentityId] [nvarchar](max) NULL,
		[FromActivityName] [nvarchar](max) NOT NULL,
		[ToActivityName] [nvarchar](max) NOT NULL,
		[ToStateName] [nvarchar](max) NULL,
		[TransitionTime] [datetime] NOT NULL,
		[TransitionClassifier] [nvarchar](max) NOT NULL,
		[IsFinalised] [bit] NOT NULL,
		[FromStateName] [nvarchar](max) NULL,
		[TriggerName] [nvarchar](max) NULL,
	 CONSTRAINT [PK_WorkflowProcessTransitionHistory] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

	PRINT 'WorkflowProcessTransitionHistory CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessInstanceStatus')
BEGIN
	CREATE TABLE [WorkflowProcessInstanceStatus](
		[Id] [uniqueidentifier] NOT NULL,
		[Status] [tinyint] NOT NULL,
		[Lock] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_WorkflowProcessInstanceStatus] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

	PRINT 'WorkflowProcessInstanceStatus CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM sys.procedures WHERE name = N'spWorkflowProcessResetRunningStatus')
BEGIN
	EXECUTE('CREATE PROCEDURE [spWorkflowProcessResetRunningStatus]
	AS
	BEGIN
		UPDATE [WorkflowProcessInstanceStatus] SET [WorkflowProcessInstanceStatus].[Status] = 2 WHERE [WorkflowProcessInstanceStatus].[Status] = 1
	END')

	PRINT 'spWorkflowProcessResetRunningStatus CREATE PROCEDURE'
END


--IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowRuntime')
--BEGIN
--	CREATE TABLE [WorkflowRuntime](
--		[RuntimeId] [uniqueidentifier] NOT NULL,
--		[Timer] [nvarchar](max) NOT NULL,
--	 CONSTRAINT [PK_WorkflowRuntime] PRIMARY KEY CLUSTERED 
--	(
--		[RuntimeId] ASC
--	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
--	) ON [PRIMARY]
--	PRINT 'WorkflowRuntime CREATE TABLE'
--END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowScheme')
BEGIN
	-- Simple schemestorage
	CREATE TABLE [WorkflowScheme](
	 [Code] [nvarchar](256) NOT NULL,
	 [Scheme] [nvarchar](max) NOT NULL,
	 CONSTRAINT [PK_WorkflowScheme] PRIMARY KEY CLUSTERED 
	(
	 [Code] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	PRINT 'WorkflowScheme CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM sys.procedures WHERE name = N'DropWorkflowProcess')
BEGIN
	EXECUTE('CREATE PROCEDURE [DropWorkflowProcess] 
		@id uniqueidentifier
	AS
	BEGIN
		BEGIN TRAN
	
		DELETE FROM dbo.WorkflowProcessInstance WHERE Id = @id
		DELETE FROM dbo.WorkflowProcessInstanceStatus WHERE Id = @id
		DELETE FROM dbo.WorkflowProcessInstancePersistence  WHERE ProcessId = @id
	
		COMMIT TRAN
	END')
	PRINT 'DropWorkflowProcess CREATE PROCEDURE'
END

IF NOT EXISTS (SELECT 1 FROM sys.procedures WHERE name = N'DropWorkflowProcesses')
BEGIN
	EXECUTE('CREATE TYPE IdsTableType AS TABLE 
	( Id uniqueidentifier );')

	PRINT 'IdsTableType CREATE TYPE'

	EXECUTE('CREATE PROCEDURE [DropWorkflowProcesses] 
		@Ids  IdsTableType	READONLY
	AS	
	BEGIN
		BEGIN TRAN
	
		DELETE dbo.WorkflowProcessInstance FROM dbo.WorkflowProcessInstance wpi  INNER JOIN @Ids  ids ON wpi.Id = ids.Id 
		DELETE dbo.WorkflowProcessInstanceStatus FROM dbo.WorkflowProcessInstanceStatus wpi  INNER JOIN @Ids  ids ON wpi.Id = ids.Id 
		DELETE dbo.WorkflowProcessInstanceStatus FROM dbo.WorkflowProcessInstancePersistence wpi  INNER JOIN @Ids  ids ON wpi.ProcessId = ids.Id 
	

		COMMIT TRAN
	END')
	PRINT 'DropWorkflowProcesses CREATE PROCEDURE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowInbox')
BEGIN
	CREATE TABLE [WorkflowInbox](
		[Id] [uniqueidentifier] NOT NULL,
		[ProcessId] [uniqueidentifier] NOT NULL,
		[IdentityId] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_WorkflowInbox] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	PRINT 'WorkflowInbox CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM sys.procedures WHERE name = N'DropWorkflowInbox')
BEGIN
	EXECUTE('CREATE PROCEDURE [DropWorkflowInbox] 
		@processId uniqueidentifier
	AS
	BEGIN
		BEGIN TRAN	
		DELETE FROM dbo.WorkflowInbox WHERE ProcessId = @processId	
		COMMIT TRAN
	END')
	PRINT 'DropWorkflowInbox CREATE PROCEDURE'
END

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

COMMIT TRANSACTION