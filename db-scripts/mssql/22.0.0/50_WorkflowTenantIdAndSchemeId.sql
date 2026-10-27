IF EXISTS(
    SELECT 1
    FROM WorkflowGlobalParameter
    WHERE LEN([Type]) > 128)
BEGIN
    RAISERROR ('BREAKING CHANGES DETECTED: Some rows in the Type column in WorkflowGlobalParameter table are longer than 128 characters. Please contact support support@optimajet.com.', 16, 1);
END

IF EXISTS(
    SELECT 1
    FROM WorkflowProcessInstance
    WHERE LEN([TenantId]) > 128)
BEGIN
    RAISERROR ('BREAKING CHANGES DETECTED: Some rows in the TenantId column in WorkflowProcessInstance table are longer than 128 characters. Please contact support support@optimajet.com.', 16, 1);
END

IF EXISTS(
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_Type_Name_Clustered'
      AND [object_id] = OBJECT_ID(N'WorkflowGlobalParameter'))
BEGIN
    DROP INDEX [IX_Type_Name_Clustered] ON WorkflowGlobalParameter;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowGlobalParameter')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowGlobalParameter
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowGlobalParameter')
      AND [name] = N'Type'
      AND [max_length] <> 256)
BEGIN
    ALTER TABLE WorkflowGlobalParameter
        ALTER COLUMN [Type] NVARCHAR(128) NOT NULL;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_Type_Name_TenantId_Clustered'
      AND [object_id] = OBJECT_ID(N'WorkflowGlobalParameter'))
BEGIN
    CREATE UNIQUE CLUSTERED INDEX [IX_Type_Name_TenantId_Clustered]
        ON WorkflowGlobalParameter ([Type], [Name], [TenantId]);
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowScheme')
      AND [name] = N'Id')
BEGIN
    ALTER TABLE WorkflowScheme
        ADD [Id] UNIQUEIDENTIFIER NULL;
END

DECLARE @workflowSchemeIdDefaultConstraintName sysname;

SELECT @workflowSchemeIdDefaultConstraintName = dc.[name]
FROM sys.default_constraints dc
INNER JOIN sys.columns c
    ON c.[default_object_id] = dc.[object_id]
WHERE dc.[parent_object_id] = OBJECT_ID(N'WorkflowScheme')
  AND c.[name] = N'Id';

IF @workflowSchemeIdDefaultConstraintName IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE WorkflowScheme DROP CONSTRAINT [' + @workflowSchemeIdDefaultConstraintName + N']');
END

EXEC(N'
UPDATE WorkflowScheme
SET [Id] = NEWID()
WHERE [Id] IS NULL;');

IF EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowScheme')
      AND [name] = N'Id'
      AND [is_nullable] = 1)
BEGIN
    ALTER TABLE WorkflowScheme
        ALTER COLUMN [Id] UNIQUEIDENTIFIER NOT NULL;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowScheme')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowScheme
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF EXISTS(
    SELECT 1
    FROM sys.key_constraints
    WHERE [name] = N'PK_WorkflowScheme'
      AND [parent_object_id] = OBJECT_ID(N'WorkflowScheme'))
BEGIN
    ALTER TABLE WorkflowScheme
        DROP CONSTRAINT [PK_WorkflowScheme];
END

ALTER TABLE WorkflowScheme
    ADD CONSTRAINT [PK_WorkflowScheme] PRIMARY KEY NONCLUSTERED ([Id]);

IF EXISTS(
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_WorkflowScheme_Code_TenantId'
      AND [object_id] = OBJECT_ID(N'WorkflowScheme'))
BEGIN
    DROP INDEX [IX_WorkflowScheme_Code_TenantId] ON WorkflowScheme;
END

CREATE UNIQUE CLUSTERED INDEX [IX_WorkflowScheme_Code_TenantId]
    ON WorkflowScheme ([Code], [TenantId]);

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowForm')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowForm
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF EXISTS(
    SELECT 1
    FROM sys.key_constraints
    WHERE [name] = N'UQ_WorkflowForm_Name_Version'
      AND [parent_object_id] = OBJECT_ID(N'WorkflowForm'))
BEGIN
    ALTER TABLE WorkflowForm
        DROP CONSTRAINT [UQ_WorkflowForm_Name_Version];
END

ALTER TABLE WorkflowForm
    ADD CONSTRAINT [UQ_WorkflowForm_Name_Version_TenantId]
        UNIQUE ([Name], [Version], [TenantId]);

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowProcessScheme')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowProcessScheme
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF EXISTS(
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_SchemeCode_IsObsolete'
      AND [object_id] = OBJECT_ID(N'WorkflowProcessScheme'))
BEGIN
    DROP INDEX [IX_SchemeCode_IsObsolete] ON WorkflowProcessScheme;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_SchemeCode_TenantId_RootSchemeId_IsObsolete'
      AND [object_id] = OBJECT_ID(N'WorkflowProcessScheme'))
BEGIN
    CREATE INDEX [IX_SchemeCode_TenantId_RootSchemeId_IsObsolete]
        ON WorkflowProcessScheme ([SchemeCode], [TenantId], [RootSchemeId], [IsObsolete]);
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowApprovalHistory')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowApprovalHistory
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowInbox')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowInbox
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowProcessInstance')
      AND [name] = N'TenantId'
      AND [max_length] <> 256)
BEGIN
    ALTER TABLE WorkflowProcessInstance
        ALTER COLUMN [TenantId] NVARCHAR(128) NULL;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowProcessInstancePersistence')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowProcessInstancePersistence
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowProcessInstanceStatus')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowProcessInstanceStatus
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowProcessTimer')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowProcessTimer
        ADD [TenantId] NVARCHAR(128) NULL;
END

IF NOT EXISTS(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'WorkflowProcessTransitionHistory')
      AND [name] = N'TenantId')
BEGIN
    ALTER TABLE WorkflowProcessTransitionHistory
        ADD [TenantId] NVARCHAR(128) NULL;
END

EXEC(N'
UPDATE statusTable
SET [TenantId] = processTable.[TenantId]
FROM WorkflowProcessInstanceStatus statusTable
INNER JOIN WorkflowProcessInstance processTable
    ON processTable.[Id] = statusTable.[Id]
WHERE processTable.[TenantId] IS NOT NULL
  AND (statusTable.[TenantId] IS NULL OR statusTable.[TenantId] <> processTable.[TenantId]);

UPDATE historyTable
SET [TenantId] = processTable.[TenantId]
FROM WorkflowApprovalHistory historyTable
INNER JOIN WorkflowProcessInstance processTable
    ON processTable.[Id] = historyTable.[ProcessId]
WHERE processTable.[TenantId] IS NOT NULL
  AND (historyTable.[TenantId] IS NULL OR historyTable.[TenantId] <> processTable.[TenantId]);

UPDATE inboxTable
SET [TenantId] = processTable.[TenantId]
FROM WorkflowInbox inboxTable
INNER JOIN WorkflowProcessInstance processTable
    ON processTable.[Id] = inboxTable.[ProcessId]
WHERE processTable.[TenantId] IS NOT NULL
  AND (inboxTable.[TenantId] IS NULL OR inboxTable.[TenantId] <> processTable.[TenantId]);

UPDATE persistenceTable
SET [TenantId] = processTable.[TenantId]
FROM WorkflowProcessInstancePersistence persistenceTable
INNER JOIN WorkflowProcessInstance processTable
    ON processTable.[Id] = persistenceTable.[ProcessId]
WHERE processTable.[TenantId] IS NOT NULL
  AND (persistenceTable.[TenantId] IS NULL OR persistenceTable.[TenantId] <> processTable.[TenantId]);

UPDATE timerTable
SET [TenantId] = processTable.[TenantId]
FROM WorkflowProcessTimer timerTable
INNER JOIN WorkflowProcessInstance processTable
    ON processTable.[Id] = timerTable.[ProcessId]
WHERE processTable.[TenantId] IS NOT NULL
  AND (timerTable.[TenantId] IS NULL OR timerTable.[TenantId] <> processTable.[TenantId]);

UPDATE transitionTable
SET [TenantId] = processTable.[TenantId]
FROM WorkflowProcessTransitionHistory transitionTable
INNER JOIN WorkflowProcessInstance processTable
    ON processTable.[Id] = transitionTable.[ProcessId]
WHERE processTable.[TenantId] IS NOT NULL
  AND (transitionTable.[TenantId] IS NULL OR transitionTable.[TenantId] <> processTable.[TenantId]);');

-- DbScriptPublisher: mark migration as applied
IF OBJECT_ID(N'[dbo].[VersionInfo]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM [dbo].[VersionInfo]
        WHERE [Version] = 50
    )
    BEGIN
        INSERT INTO [dbo].[VersionInfo] (
            [Version],
            [AppliedOn],
            [Description]
        )
        VALUES (
            50,
            GETDATE(),
            N'Migration50WorkflowTenantIdAndSchemeId'
        );
    END;
END;
