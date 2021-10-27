--WorkflowProcessInstance
IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstance' 
AND [COLUMN_NAME] = N'ParentProcessId')
BEGIN
	ALTER TABLE WorkflowProcessInstance ADD ParentProcessId uniqueidentifier NULL
	PRINT 'ADD WorkflowProcessInstance.ParentProcessId type uniqueidentifier'
END
GO
IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstance' 
AND [COLUMN_NAME] = N'RootProcessId')
BEGIN
	ALTER TABLE WorkflowProcessInstance ADD RootProcessId uniqueidentifier NULL
	PRINT 'ADD WorkflowProcessInstance.RootProcessId type uniqueidentifier'
END
GO

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstance' AND [COLUMN_NAME] = N'RootProcessId' AND [IS_NULLABLE] = 'YES')
BEGIN
	UPDATE WorkflowProcessInstance SET RootProcessId = Id WHERE RootProcessId IS NULL 
	ALTER TABLE WorkflowProcessInstance ALTER COLUMN RootProcessId uniqueidentifier NOT NULL
	PRINT 'ALTER WorkflowProcessInstance.RootProcessId type uniqueidentifier NOT NULL -> NULL'
END
GO

--WorkflowProcessScheme
IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessScheme' 
AND [COLUMN_NAME] = N'RootSchemeCode')
BEGIN
	ALTER TABLE WorkflowProcessScheme ADD RootSchemeCode nvarchar (max) NULL
	PRINT 'ADD WorkflowProcessScheme.RootSchemeCode type nvarchar(max)'
END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessScheme' 
AND [COLUMN_NAME] = N'RootSchemeId')
BEGIN
	ALTER TABLE WorkflowProcessScheme ADD RootSchemeId uniqueidentifier NULL
	PRINT 'ADD WorkflowProcessScheme.RootSchemeId type  uniqueidentifier'
END
GO
IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessScheme' 
AND [COLUMN_NAME] = N'AllowedActivities')
BEGIN
	ALTER TABLE WorkflowProcessScheme ADD AllowedActivities nvarchar (max) NULL
	PRINT 'ADD WorkflowProcessScheme.AllowedActivities type nvarchar(max)'
END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessScheme' 
AND [COLUMN_NAME] = N'StartingTransition')
BEGIN
	ALTER TABLE WorkflowProcessScheme ADD StartingTransition nvarchar (max) NULL
	PRINT 'ADD WorkflowProcessScheme.StartingTransition type nvarchar(max)'
END
GO