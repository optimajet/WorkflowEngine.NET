IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstance' 
AND [COLUMN_NAME] = N'SubprocessName')
BEGIN
	ALTER TABLE WorkflowProcessInstance ADD SubprocessName nvarchar (max) NULL
	PRINT 'ADD WorkflowProcessInstance.SubprocessName type nvarchar(max) NULL'
END
GO

UPDATE WorkflowProcessInstance SET SubprocessName = StartingTransition
GO