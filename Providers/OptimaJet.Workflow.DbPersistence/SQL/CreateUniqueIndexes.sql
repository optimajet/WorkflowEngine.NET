IF NOT EXISTS(SELECT DISTINCT 1
              FROM WorkflowProcessInstancePersistence
              WHERE LEN(ParameterName) > 900)
    BEGIN
        IF NOT EXISTS(SELECT DISTINCT 1 FROM WorkflowProcessTimer WHERE LEN(Name) > 900)
            BEGIN
                PRINT 'OK: No breaking changes detected'
            END
        ELSE
            BEGIN
                RAISERROR ('BREAKING CHANGES DETECTED: Some rows in the Name column in WorkflowProcessTimer table is too long. Please contact support support@optimajet.com.', 16, 1)
            END
    END
ELSE
    BEGIN
        RAISERROR ('BREAKING CHANGES DETECTED: Some rows in the ParameterName column in WorkflowProcessInstancePersistence table is too long. Please contact support support@optimajet.com.', 16, 1)
    END

IF NOT EXISTS(SELECT max_length
              FROM sys.columns
              WHERE columns.object_id = OBJECT_ID('WorkflowProcessInstancePersistence')
                AND columns.name = 'ParameterName'
                AND columns.max_length = 1800)
    BEGIN
        ALTER TABLE WorkflowProcessInstancePersistence
            ALTER COLUMN ParameterName NVARCHAR(900) NOT NULL;
    END

IF NOT EXISTS(SELECT *
              FROM sys.indexes
              WHERE name = 'IX_ProcessId_ParameterName'
                AND object_id = OBJECT_ID('WorkflowProcessInstancePersistence'))
    BEGIN
        CREATE UNIQUE INDEX IX_ProcessId_ParameterName ON WorkflowProcessInstancePersistence (ProcessId, ParameterName);
    END

IF NOT EXISTS(SELECT *
              FROM sys.indexes
              WHERE name = 'IX_ProcessId_IdentityId'
                AND object_id = OBJECT_ID('WorkflowInbox'))
    BEGIN
        CREATE UNIQUE INDEX IX_ProcessId_IdentityId ON WorkflowInbox (ProcessId, IdentityId);
    END

IF NOT EXISTS(SELECT max_length
              FROM sys.columns
              WHERE columns.object_id = OBJECT_ID('WorkflowProcessTimer')
                AND columns.name = 'Name'
                AND columns.max_length = 1800)
    BEGIN
        ALTER TABLE WorkflowProcessTimer
            ALTER COLUMN Name NVARCHAR(900) NOT NULL;
    END

IF NOT EXISTS(SELECT *
              FROM sys.indexes
              WHERE name = 'IX_ProcessId_Name'
                AND object_id = OBJECT_ID('WorkflowProcessTimer'))
    BEGIN
        CREATE UNIQUE INDEX IX_ProcessId_Name ON WorkflowProcessTimer (ProcessId, Name);
    END
