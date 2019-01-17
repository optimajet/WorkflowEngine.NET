/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for SQLServer
Version: 3.1
File: update_3_1.sql
*/

-- WorkflowProcessScheme
ALTER TABLE WorkflowProcessScheme ALTER COLUMN SchemeCode NVARCHAR(256) NOT NULL
GO

ALTER TABLE WorkflowProcessScheme ALTER COLUMN RootSchemeCode NVARCHAR(256) NULL
GO

ALTER TABLE WorkflowProcessScheme ALTER COLUMN DefiningParametersHash NVARCHAR(24) NOT NULL
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_SchemeCode_Hash_IsObsolete'
			AND object_id = OBJECT_ID('WorkflowProcessScheme')
		)
BEGIN
	CREATE NONCLUSTERED INDEX IX_SchemeCode_Hash_IsObsolete ON WorkflowProcessScheme (
		SchemeCode ASC,
    DefiningParametersHash ASC,
		IsObsolete ASC
		)
END
GO

-- WorkflowGlobalParameter
ALTER TABLE WorkflowGlobalParameter DROP CONSTRAINT PK_WorkflowGlobalParameter
GO

ALTER TABLE WorkflowGlobalParameter ADD CONSTRAINT PK_WorkflowGlobalParameter PRIMARY KEY NONCLUSTERED (Id)
GO

ALTER TABLE WorkflowGlobalParameter ALTER COLUMN Type NVARCHAR(306) NOT NULL
GO

ALTER TABLE WorkflowGlobalParameter ALTER COLUMN Name NVARCHAR(128) NOT NULL
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_Type_Name_Clustered'
			AND object_id = OBJECT_ID('WorkflowGlobalParameter')
		)
BEGIN
	CREATE UNIQUE CLUSTERED INDEX IX_Type_Name_Clustered ON WorkflowGlobalParameter (
		Type ASC
		,Name ASC
		)
END
GO

-- WorkflowProcessInstancePersistence
ALTER TABLE WorkflowProcessInstancePersistence DROP CONSTRAINT PK_WorkflowProcessInstancePersistence
GO

ALTER TABLE WorkflowProcessInstancePersistence ADD CONSTRAINT PK_WorkflowProcessInstancePersistence PRIMARY KEY NONCLUSTERED (Id)
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_ProcessId_Clustered'
			AND object_id = OBJECT_ID('WorkflowProcessInstancePersistence')
		)
BEGIN
	CREATE CLUSTERED INDEX IX_ProcessId_Clustered ON WorkflowProcessInstancePersistence (ProcessId ASC)
END
GO

ALTER TABLE WorkflowProcessInstancePersistence ALTER COLUMN Value NVARCHAR(MAX) NOT NULL
GO

-- WorkflowProcessTransitionHistory
ALTER TABLE WorkflowProcessTransitionHistory DROP CONSTRAINT PK_WorkflowProcessTransitionHistory
GO

ALTER TABLE WorkflowProcessTransitionHistory ADD CONSTRAINT PK_WorkflowProcessTransitionHistory PRIMARY KEY NONCLUSTERED (Id)
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_ProcessId_Clustered'
			AND object_id = OBJECT_ID('WorkflowProcessTransitionHistory')
		)
BEGIN
	CREATE CLUSTERED INDEX IX_ProcessId_Clustered ON WorkflowProcessTransitionHistory (ProcessId ASC)
END
GO

ALTER TABLE WorkflowProcessTransitionHistory ALTER COLUMN ExecutorIdentityId NVARCHAR(256) NULL
GO

ALTER TABLE WorkflowProcessTransitionHistory ALTER COLUMN ActorIdentityId NVARCHAR(256) NULL
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_ExecutorIdentityId'
			AND object_id = OBJECT_ID('WorkflowProcessTransitionHistory')
		)
BEGIN
	CREATE NONCLUSTERED INDEX IX_ExecutorIdentityId ON WorkflowProcessTransitionHistory (ExecutorIdentityId ASC)
END
GO

-- WorkflowProcessTimer
ALTER TABLE WorkflowProcessTimer DROP CONSTRAINT PK_WorkflowProcessTimer
GO

ALTER TABLE WorkflowProcessTimer ADD CONSTRAINT PK_WorkflowProcessTimer PRIMARY KEY NONCLUSTERED (Id)
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_NextExecutionDateTime_Clustered'
			AND object_id = OBJECT_ID('WorkflowProcessTimer')
		)
BEGIN
	CREATE CLUSTERED INDEX IX_NextExecutionDateTime_Clustered ON WorkflowProcessTimer (NextExecutionDateTime ASC)
END
GO

-- WorkflowInbox
ALTER TABLE WorkflowInbox DROP CONSTRAINT PK_WorkflowInbox
GO

ALTER TABLE WorkflowInbox ADD CONSTRAINT PK_WorkflowInbox PRIMARY KEY NONCLUSTERED (Id)
GO

ALTER TABLE WorkflowInbox ALTER COLUMN IdentityId NVARCHAR(256) NOT NULL
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_IdentityId_Clustered'
			AND object_id = OBJECT_ID('WorkflowInbox')
		)
BEGIN
	CREATE CLUSTERED INDEX IX_IdentityId_Clustered ON WorkflowInbox (IdentityId ASC)
END
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_ProcessId'
			AND object_id = OBJECT_ID('WorkflowInbox')
		)
BEGIN
	CREATE NONCLUSTERED INDEX IX_ProcessId ON WorkflowInbox (ProcessId ASC)
END
GO