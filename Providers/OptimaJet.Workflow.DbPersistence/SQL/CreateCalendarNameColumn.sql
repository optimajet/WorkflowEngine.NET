IF NOT EXISTS(SELECT 1
              FROM sys.columns
              WHERE columns.object_id = OBJECT_ID('WorkflowProcessInstance')
                AND columns.name = 'CalendarName')
    BEGIN
        ALTER TABLE WorkflowProcessInstance
            ADD CalendarName NVARCHAR(450);
    END

IF NOT EXISTS(SELECT 1
              FROM sys.indexes
              WHERE name = 'IX_CalendarName'
                AND object_id = OBJECT_ID('WorkflowProcessInstance'))
    BEGIN
        CREATE INDEX IX_CalendarName ON WorkflowProcessInstance (CalendarName);
    END
