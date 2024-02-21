/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MSSQL and Azure SQL
Version: 12.2
File: CreatePersistenceObjects.sql

*/
BEGIN TRANSACTION

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessScheme'
		)
BEGIN
	CREATE TABLE WorkflowProcessScheme (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessScheme PRIMARY KEY
		,[Scheme] NTEXT NOT NULL
		,[DefiningParameters] NTEXT NOT NULL
		,[DefiningParametersHash] NVARCHAR(24) NOT NULL
		,[SchemeCode] NVARCHAR(256) NOT NULL
		,[IsObsolete] BIT DEFAULT 0 NOT NULL
		,[RootSchemeCode] NVARCHAR(256)
		,[RootSchemeId] UNIQUEIDENTIFIER
		,[AllowedActivities] NVARCHAR(max)
		,[StartingTransition] NVARCHAR(max)
		)

	CREATE INDEX IX_SchemeCode_Hash_IsObsolete ON WorkflowProcessScheme (
		SchemeCode
		,DefiningParametersHash
		,IsObsolete
		)

	PRINT 'WorkflowProcessScheme CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessInstance'
		)
BEGIN
	CREATE TABLE WorkflowProcessInstance (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessInstance PRIMARY KEY
		,[StateName] NVARCHAR(max)
		,[ActivityName] NVARCHAR(max) NOT NULL
		,[SchemeId] UNIQUEIDENTIFIER
		,[PreviousState] NVARCHAR(max)
		,[PreviousStateForDirect] NVARCHAR(max)
		,[PreviousStateForReverse] NVARCHAR(max)
		,[PreviousActivity] NVARCHAR(max)
		,[PreviousActivityForDirect] NVARCHAR(max)
		,[PreviousActivityForReverse] NVARCHAR(max)
		,[ParentProcessId] UNIQUEIDENTIFIER
		,[RootProcessId] UNIQUEIDENTIFIER NOT NULL
		,[IsDeterminingParametersChanged] BIT DEFAULT 0 NOT NULL
        ,[TenantId] NVARCHAR(1024)
		,[StartingTransition] NVARCHAR(max)
		,[SubprocessName] NVARCHAR(max)
		,[CreationDate] datetime NOT NULL CONSTRAINT DF_WorkflowProcessInstance_CreationDate DEFAULT getdate()
		,[LastTransitionDate] datetime NULL
	    ,[CalendarName] NVARCHAR(450)
		)

	CREATE INDEX IX_RootProcessId ON WorkflowProcessInstance (RootProcessId)
	
	CREATE INDEX IX_CalendarName ON WorkflowProcessInstance (CalendarName)

	PRINT 'WorkflowProcessInstance CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessInstancePersistence'
		)
BEGIN
	CREATE TABLE WorkflowProcessInstancePersistence (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessInstancePersistence PRIMARY KEY NONCLUSTERED
		,[ProcessId] UNIQUEIDENTIFIER NOT NULL
		,[ParameterName] NVARCHAR(max) NOT NULL
		,[Value] NVARCHAR(max) NOT NULL
		)

	CREATE CLUSTERED INDEX IX_ProcessId_Clustered ON WorkflowProcessInstancePersistence (ProcessId)

	PRINT 'WorkflowProcessInstancePersistence CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory'
		)
BEGIN
	CREATE TABLE WorkflowProcessTransitionHistory (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessTransitionHistory PRIMARY KEY NONCLUSTERED
		,[ProcessId] UNIQUEIDENTIFIER NOT NULL
		,[ExecutorIdentityId] NVARCHAR(256)
		,[ActorIdentityId] NVARCHAR(256)
        ,[ExecutorName] NVARCHAR(256)
        ,[ActorName] NVARCHAR(256)
		,[FromActivityName] NVARCHAR(max) NOT NULL
		,[ToActivityName] NVARCHAR(max) NOT NULL
		,[ToStateName] NVARCHAR(max)
		,[TransitionTime] DATETIME NOT NULL
		,[TransitionClassifier] NVARCHAR(max) NOT NULL
		,[IsFinalised] BIT NOT NULL
		,[FromStateName] NVARCHAR(max)
		,[TriggerName] NVARCHAR(max)
		,[StartTransitionTime] DATETIME
		,[TransitionDuration] BIGINT
		)

	CREATE CLUSTERED INDEX IX_ProcessId_Clustered ON WorkflowProcessTransitionHistory (ProcessId)

	CREATE INDEX IX_ExecutorIdentityId ON WorkflowProcessTransitionHistory (ExecutorIdentityId)

	PRINT 'WorkflowProcessTransitionHistory CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessInstanceStatus'
		)
BEGIN
	CREATE TABLE WorkflowProcessInstanceStatus (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessInstanceStatus PRIMARY KEY
		,[Status] TINYINT NOT NULL
		,[Lock] UNIQUEIDENTIFIER NOT NULL
		,[RuntimeId] nvarchar(450) NOT NULL
		,[SetTime] datetime NOT NULL
		)

	CREATE NONCLUSTERED INDEX [IX_WorkflowProcessInstanceStatus_Status] ON [dbo].[WorkflowProcessInstanceStatus]
	(
		[Status] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];


	CREATE NONCLUSTERED INDEX [IX_WorkflowProcessInstanceStatus_Status_Runtime] ON [dbo].[WorkflowProcessInstanceStatus]
	(
		[Status] ASC,
		[RuntimeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];


	PRINT 'WorkflowProcessInstanceStatus CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.procedures
		WHERE name = N'spWorkflowProcessResetRunningStatus'
		)
BEGIN
	EXECUTE (
			'CREATE PROCEDURE [spWorkflowProcessResetRunningStatus]
	AS
	BEGIN
		UPDATE [WorkflowProcessInstanceStatus] SET [WorkflowProcessInstanceStatus].[Status] = 2 WHERE [WorkflowProcessInstanceStatus].[Status] = 1
	END'
			)

	PRINT 'spWorkflowProcessResetRunningStatus CREATE PROCEDURE'
END

IF NOT EXISTS (
        SELECT 1
        FROM sys.procedures
        WHERE name = N'DropUnusedWorkflowProcessScheme'
    )
    BEGIN
        EXECUTE (
            'CREATE PROCEDURE [DropUnusedWorkflowProcessScheme]
    AS
    BEGIN
        DELETE wps FROM WorkflowProcessScheme AS wps 
            WHERE wps.IsObsolete = 1 
                AND NOT EXISTS (SELECT * FROM WorkflowProcessInstance wpi WHERE wpi.SchemeId = wps.Id )

        RETURN (SELECT COUNT(*) 
            FROM WorkflowProcessInstance wpi LEFT OUTER JOIN WorkflowProcessScheme wps ON wpi.SchemeId = wps.Id 
            WHERE wps.Id IS NULL)
    END'
            )

        PRINT 'DropUnusedWorkflowProcessScheme CREATE PROCEDURE'
    END


IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowScheme'
		)
BEGIN
	CREATE TABLE WorkflowScheme (
		[Code] NVARCHAR(256) NOT NULL CONSTRAINT PK_WorkflowScheme PRIMARY KEY,
		[Scheme] NVARCHAR(max) NOT NULL,
		[CanBeInlined] [bit] NOT NULL DEFAULT(0),
		[InlinedSchemes] [nvarchar](max) NULL,
        [Tags] [nvarchar](max) NULL,
		)

	PRINT 'WorkflowScheme CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowInbox'
		)
BEGIN
	CREATE TABLE WorkflowInbox (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowInbox PRIMARY KEY NONCLUSTERED
		,[ProcessId] UNIQUEIDENTIFIER NOT NULL
		,[IdentityId] NVARCHAR(256) NOT NULL
		,[AddingDate] DATETIME NOT NULL DEFAULT GETDATE()
		,[AvailableCommands] NVARCHAR(max) NOT NULL DEFAULT ''
		)

	CREATE CLUSTERED INDEX IX_IdentityId_Clustered ON WorkflowInbox (IdentityId)

	CREATE INDEX IX_ProcessId ON WorkflowInbox (ProcessId)

	PRINT 'WorkflowInbox CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.procedures
		WHERE name = N'DropWorkflowInbox'
		)
BEGIN
	EXECUTE (
			'CREATE PROCEDURE [DropWorkflowInbox]
		@processId uniqueidentifier
	AS
	BEGIN
		BEGIN TRAN
		DELETE FROM dbo.WorkflowInbox WHERE ProcessId = @processId
		COMMIT TRAN
	END'
			)

	PRINT 'DropWorkflowInbox CREATE PROCEDURE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessTimer'
		)
BEGIN
	CREATE TABLE WorkflowProcessTimer (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessTimer PRIMARY KEY NONCLUSTERED
		,[ProcessId] UNIQUEIDENTIFIER NOT NULL
        ,[RootProcessId] UNIQUEIDENTIFIER NOT NULL
		,[Name] NVARCHAR(max) NOT NULL
		,[NextExecutionDateTime] DATETIME NOT NULL
		,[Ignore] BIT NOT NULL
		)

	CREATE CLUSTERED INDEX IX_NextExecutionDateTime_Clustered ON WorkflowProcessTimer (NextExecutionDateTime)
    CREATE INDEX IX_ProcessId ON WorkflowProcessTimer (ProcessId)

	PRINT 'WorkflowProcessTimer CREATE TABLE'
END


IF NOT EXISTS (
        SELECT 1
        FROM [INFORMATION_SCHEMA].[TABLES]
        WHERE [TABLE_NAME] = N'WorkflowProcessAssignment'
    )
    BEGIN
        CREATE TABLE WorkflowProcessAssignment (
            [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessAssignment PRIMARY KEY NONCLUSTERED
            ,[AssignmentCode] NVARCHAR(2048) NOT NULL
            ,[ProcessId] UNIQUEIDENTIFIER NOT NULL
            ,[Name] NVARCHAR(max) NOT NULL
            ,[Description] NVARCHAR(max)
            ,[StatusState] NVARCHAR(max) NOT NULL
            ,[DateCreation] DATETIME NOT NULL
            ,[DateStart] DATETIME
            ,[DateFinish] DATETIME
            ,[DeadlineToStart] DATETIME
            ,[DeadlineToComplete] DATETIME
            ,[Executor] NVARCHAR(256) NOT NULL
            ,[Observers] NVARCHAR(max)
            ,[Tags] NVARCHAR(max)
            ,[IsActive] BIT NOT NULL
            ,[IsDeleted] BIT NOT NULL
        )

        CREATE INDEX IX_Assignment_ProcessId ON WorkflowProcessAssignment (ProcessId)
        CREATE INDEX IX_Assignment_AssignmentCode ON WorkflowProcessAssignment (AssignmentCode)
        CREATE INDEX IX_Assignment_Executor ON WorkflowProcessAssignment (Executor)
        CREATE INDEX IX_Assignment_ProcessId_Executor ON WorkflowProcessAssignment (ProcessId, Executor)

        PRINT 'WorkflowProcessAssignment CREATE TABLE'
    END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowGlobalParameter'
		)
BEGIN
	CREATE TABLE WorkflowGlobalParameter (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowGlobalParameter PRIMARY KEY NONCLUSTERED
		,[Type] NVARCHAR(306) NOT NULL
		,[Name] NVARCHAR(128) NOT NULL
		,[Value] NVARCHAR(max) NOT NULL
		)

	CREATE UNIQUE CLUSTERED INDEX IX_Type_Name_Clustered ON WorkflowGlobalParameter (
		Type
		,Name
		)

	PRINT 'WorkflowGlobalParameter CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowRuntime'
		)
BEGIN
	CREATE TABLE WorkflowRuntime (
		[RuntimeId] nvarchar(450) NOT NULL CONSTRAINT PK_WorkflowRuntime PRIMARY KEY
		,[Lock] UNIQUEIDENTIFIER NOT NULL
		,[Status] TINYINT NOT NULL
		,[RestorerId] nvarchar(450)
		,[NextTimerTime] datetime
		,[NextServiceTimerTime] datetime
		,[LastAliveSignal] datetime
		)

	PRINT 'WorkflowRuntime CREATE TABLE'

    EXEC('INSERT INTO WorkflowRuntime  (RuntimeId,Lock,Status)  VALUES (''00000000-0000-0000-0000-000000000000'', NEWID(),100)');
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowSync'
		)
BEGIN
	CREATE TABLE WorkflowSync (
		[Name] nvarchar(450) NOT NULL CONSTRAINT PK_WorkflowSync PRIMARY KEY
		,[Lock] UNIQUEIDENTIFIER NOT NULL
		)

	INSERT INTO [dbo].[WorkflowSync]
           ([Name]
           ,[Lock])
     VALUES
           ('Timer',
           NEWID());

	INSERT INTO [dbo].[WorkflowSync]
           ([Name]
           ,[Lock])
     VALUES
           ('ServiceTimer',
           NEWID());

	PRINT 'WorkflowSync CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowApprovalHistory'
		)
BEGIN
CREATE TABLE [dbo].[WorkflowApprovalHistory](
	[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowApprovalHistory PRIMARY KEY NONCLUSTERED
	,[ProcessId] UNIQUEIDENTIFIER NOT NULL
	,[IdentityId] NVARCHAR(256) NULL
	,[AllowedTo] NVARCHAR(max) NULL
	,[TransitionTime] DateTime NULL
	,[Sort] BIGINT NULL
	,[InitialState] NVARCHAR(1024) NOT NULL
	,[DestinationState] NVARCHAR(1024) NOT NULL
	,[TriggerName] NVARCHAR(1024) NULL
	,[Commentary] NVARCHAR(max) NULL
    )

	CREATE CLUSTERED INDEX IX_ProcessId_Clustered ON WorkflowApprovalHistory (ProcessId)
	CREATE NONCLUSTERED INDEX IX_IdentityId ON WorkflowApprovalHistory (IdentityId)
	PRINT 'WorkflowApprovalHistory CREATE TABLE'
END



COMMIT TRANSACTION