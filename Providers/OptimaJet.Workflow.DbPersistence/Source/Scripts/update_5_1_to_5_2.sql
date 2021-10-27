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

    PRINT 'WorkflowProcessAssignment CREATE TABLE'
END
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_Assignment_ProcessId'
			AND object_id = OBJECT_ID('WorkflowProcessAssignment')
		)
BEGIN
CREATE INDEX IX_Assignment_ProcessId ON WorkflowProcessAssignment (ProcessId)
END
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_Assignment_AssignmentCode'
			AND object_id = OBJECT_ID('WorkflowProcessAssignment')
		)
BEGIN
CREATE INDEX IX_Assignment_AssignmentCode ON WorkflowProcessAssignment (AssignmentCode)
END
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_Assignment_Executor'
			AND object_id = OBJECT_ID('WorkflowProcessAssignment')
		)
BEGIN
CREATE INDEX IX_Assignment_Executor ON WorkflowProcessAssignment (Executor)
END
GO

IF NOT EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_Assignment_ProcessId_Executor'
          AND object_id = OBJECT_ID('WorkflowProcessAssignment')
    )
    BEGIN
        CREATE INDEX IX_Assignment_ProcessId_Executor ON WorkflowProcessAssignment (ProcessId, Executor)
    END
GO

IF NOT EXISTS(SELECT 1
              FROM [INFORMATION_SCHEMA].[COLUMNS]
              WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory'
                AND [COLUMN_NAME] = N'ExecutorName')
BEGIN
ALTER TABLE WorkflowProcessTransitionHistory
    ADD ExecutorName NVARCHAR(256)
    PRINT 'ADD WorkflowProcessTransitionHistory.ExecutorName type NVARCHAR(256) NULL'
END
GO

IF NOT EXISTS(SELECT 1
              FROM [INFORMATION_SCHEMA].[COLUMNS]
              WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory'
                AND [COLUMN_NAME] = N'ActorName')
    BEGIN
        ALTER TABLE WorkflowProcessTransitionHistory
            ADD ActorName NVARCHAR(256)
        PRINT 'ADD WorkflowProcessTransitionHistory.ActorName type NVARCHAR(256) NULL'
    END
GO
    
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
