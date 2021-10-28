IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstance' 
AND [COLUMN_NAME] = N'TenantId')
BEGIN
	ALTER TABLE WorkflowProcessInstance ADD TenantId nvarchar (1024) NULL
	PRINT 'ADD WorkflowProcessInstance.TenantId type nvarchar(1024) NULL'
END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowScheme' 
AND [COLUMN_NAME] = N'Tags')
BEGIN
    ALTER TABLE WorkflowScheme ADD Tags nvarchar(max) NULL
    PRINT 'ADD WorkflowScheme.Tags type nvarchar(max) NULL'
END
GO