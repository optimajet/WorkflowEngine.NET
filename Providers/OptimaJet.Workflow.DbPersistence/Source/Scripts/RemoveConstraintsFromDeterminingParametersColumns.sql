/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MSSQL and Azure SQL
Version: 21.0
File: RemoveConstraintsFromDeterminingParametersColumns.sql
Description: Removes constraints and indexes from deprecated determining parameters columns.
             Columns are kept for backward compatibility until next release.
*/

-- Drop index IX_SchemeCode_Hash_IsObsolete that uses DefiningParametersHash
IF EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_SchemeCode_Hash_IsObsolete' 
        AND object_id = OBJECT_ID('WorkflowProcessScheme')
)
BEGIN
    DROP INDEX IX_SchemeCode_Hash_IsObsolete ON WorkflowProcessScheme
    PRINT 'IX_SchemeCode_Hash_IsObsolete index dropped'
END

-- Recreate index without DefiningParametersHash
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_SchemeCode_IsObsolete' 
        AND object_id = OBJECT_ID('WorkflowProcessScheme')
)
BEGIN
    CREATE INDEX IX_SchemeCode_IsObsolete ON WorkflowProcessScheme (
        SchemeCode,
        IsObsolete
    )
    PRINT 'IX_SchemeCode_IsObsolete index created'
END

-- Remove NOT NULL constraint from DefiningParametersHash
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('WorkflowProcessScheme') 
        AND name = 'DefiningParametersHash'
        AND is_nullable = 0
)
BEGIN
    ALTER TABLE WorkflowProcessScheme ALTER COLUMN DefiningParametersHash NVARCHAR(24) NULL
    PRINT 'WorkflowProcessScheme.DefiningParametersHash changed to nullable'
END

-- Remove NOT NULL constraint from DefiningParameters
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('WorkflowProcessScheme') 
        AND name = 'DefiningParameters'
        AND is_nullable = 0
)
BEGIN
    ALTER TABLE WorkflowProcessScheme ALTER COLUMN DefiningParameters NTEXT NULL
    PRINT 'WorkflowProcessScheme.DefiningParameters changed to nullable'
END

-- Drop default constraint from WorkflowProcessInstance.IsDeterminingParametersChanged
DECLARE @ConstraintName NVARCHAR(200)
SELECT @ConstraintName = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID('WorkflowProcessInstance')
    AND c.name = 'IsDeterminingParametersChanged'

IF @ConstraintName IS NOT NULL
BEGIN
    DECLARE @DropConstraintSQL NVARCHAR(MAX)
    SET @DropConstraintSQL = 'ALTER TABLE WorkflowProcessInstance DROP CONSTRAINT ' + @ConstraintName
    EXEC sp_executesql @DropConstraintSQL
    PRINT 'WorkflowProcessInstance.IsDeterminingParametersChanged default constraint dropped: ' + @ConstraintName
END

-- Remove NOT NULL constraint from IsDeterminingParametersChanged
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('WorkflowProcessInstance') 
        AND name = 'IsDeterminingParametersChanged'
        AND is_nullable = 0
)
BEGIN
    ALTER TABLE WorkflowProcessInstance ALTER COLUMN IsDeterminingParametersChanged BIT NULL
    PRINT 'WorkflowProcessInstance.IsDeterminingParametersChanged changed to nullable'
END

PRINT 'RemoveConstraintsFromDeterminingParametersColumns migration completed successfully'
PRINT 'Note: Columns are kept for backward compatibility and will be removed in the next release'
