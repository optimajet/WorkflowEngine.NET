IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstance' 
AND [COLUMN_NAME] = N'CreationDate')
BEGIN
	ALTER TABLE WorkflowProcessInstance ADD
	CreationDate datetime NOT NULL CONSTRAINT DF_WorkflowProcessInstance_CreationDate DEFAULT getdate()
	PRINT 'ADD WorkflowProcessInstance.CreationDate type datetime NOT NULL'
END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstance' 
AND [COLUMN_NAME] = N'LastTransitionDate')
BEGIN
	ALTER TABLE WorkflowProcessInstance ADD
	LastTransitionDate datetime NULL
	PRINT 'ADD WorkflowProcessInstance.LastTransitionDate type datetime NULL'
END
GO

IF NOT EXISTS(SELECT 1
              FROM [INFORMATION_SCHEMA].[COLUMNS]
              WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory'
                AND [COLUMN_NAME] = N'StartTransitionTime')
    BEGIN
        ALTER TABLE WorkflowProcessTransitionHistory
            ADD StartTransitionTime datetime
        PRINT 'ADD WorkflowProcessTransitionHistory.StartTransitionTime type datetime NULL'
    END
GO

IF NOT EXISTS(SELECT 1
              FROM [INFORMATION_SCHEMA].[COLUMNS]
              WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory'
                AND [COLUMN_NAME] = N'TransitionDuration')
    BEGIN
        ALTER TABLE WorkflowProcessTransitionHistory
            ADD TransitionDuration bigint
        PRINT 'ADD WorkflowProcessTransitionHistory.TransitionDuration type bigint NULL'
    END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowInbox' 
AND [COLUMN_NAME] = N'AddingDate')
BEGIN
	ALTER TABLE WorkflowInbox ADD
	AddingDate datetime NOT NULL DEFAULT GETDATE()
	PRINT 'ADD WorkflowInbox.AddingDate type datetime NOT NULL'
END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowInbox' 
AND [COLUMN_NAME] = N'AvailableCommands')
BEGIN
	ALTER TABLE WorkflowInbox ADD
	AvailableCommands NVARCHAR(max) NOT NULL DEFAULT ''
	PRINT 'ADD WorkflowInbox.AvailableCommands type NVARCHAR(max) NOT NULL'
END
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_IdentityId'
			AND object_id = OBJECT_ID('WorkflowApprovalHistory')
		)
BEGIN
	CREATE NONCLUSTERED INDEX IX_IdentityId ON WorkflowApprovalHistory (IdentityId)
END
GO