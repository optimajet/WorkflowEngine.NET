/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MSSQL and Azure SQL
Version: 4.1
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
		)

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
		,[FromActivityName] NVARCHAR(max) NOT NULL
		,[ToActivityName] NVARCHAR(max) NOT NULL
		,[ToStateName] NVARCHAR(max)
		,[TransitionTime] DATETIME NOT NULL
		,[TransitionClassifier] NVARCHAR(max) NOT NULL
		,[IsFinalised] BIT NOT NULL
		,[FromStateName] NVARCHAR(max)
		,[TriggerName] NVARCHAR(max)
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
		)

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
		,[Name] NVARCHAR(max) NOT NULL
		,[NextExecutionDateTime] DATETIME NOT NULL
		,[Ignore] BIT NOT NULL
		)

	CREATE CLUSTERED INDEX IX_NextExecutionDateTime_Clustered ON WorkflowProcessTimer (NextExecutionDateTime)

	PRINT 'WorkflowProcessTimer CREATE TABLE'
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

COMMIT TRANSACTION