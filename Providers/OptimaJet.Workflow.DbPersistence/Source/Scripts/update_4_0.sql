/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for SQLServer
Version: 4.0
File: update_4_0.sql
*/

--WorkflowScheme
IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowScheme' 
AND [COLUMN_NAME] = N'CanBeInlined')
BEGIN
	ALTER TABLE WorkflowScheme ADD CanBeInlined [bit] NOT NULL DEFAULT(0)
	PRINT 'ADD WorkflowScheme.CanBeInlined type bit'
END
GO


IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowScheme' 
AND [COLUMN_NAME] = N'InlinedSchemes')
BEGIN
	ALTER TABLE WorkflowScheme ADD InlinedSchemes [nvarchar](max) NULL
	PRINT 'ADD WorkflowScheme.InlinedSchemes type [nvarchar](max)'
END
GO