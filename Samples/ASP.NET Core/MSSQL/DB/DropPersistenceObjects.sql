/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MSSQL
Version: 12.2
File: DropPersistenceObjects.sql
*/

BEGIN TRANSACTION

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessScheme')
BEGIN
	DROP TABLE [WorkflowProcessScheme]
	PRINT 'WorkflowProcessScheme DROP TABLE'
END

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessInstance')
BEGIN
	DROP TABLE [WorkflowProcessInstance]
	PRINT 'WorkflowProcessInstance DROP DROP'
END

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessInstancePersistence')
BEGIN
	DROP TABLE [WorkflowProcessInstancePersistence]
	PRINT 'WorkflowProcessInstancePersistence DROP TABLE'
END

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory')
BEGIN
	DROP TABLE [WorkflowProcessTransitionHistory]
	PRINT 'WorkflowProcessTransitionHistory DROP TABLE'
END

IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = N'tWorkflowProcessTransitionHistoryInsert')
BEGIN
	DROP TRIGGER [tWorkflowProcessTransitionHistoryInsert]
	PRINT 'WorkflowProcessTransitionHistory DROP TRIGGER tWorkflowProcessTransitionHistoryInsert'
END

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessInstanceStatus')
BEGIN
	DROP TABLE [WorkflowProcessInstanceStatus]
	PRINT 'WorkflowProcessInstanceStatus DROP TABLE'
END

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = N'spWorkflowProcessResetRunningStatus')
BEGIN
	DROP PROCEDURE [spWorkflowProcessResetRunningStatus]
	PRINT 'spWorkflowProcessResetRunningStatus DROP PROCEDURE'
END


IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowRuntime')
BEGIN
	DROP TABLE [WorkflowRuntime]
	PRINT 'WorkflowRuntime DROP TABLE'
END

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowScheme')
BEGIN
	DROP TABLE [WorkflowScheme]
	PRINT 'WorkflowScheme DROP TABLE'
END

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowInbox')
BEGIN
	DROP TABLE [WorkflowInbox]
	PRINT 'WorkflowInbox DROP TABLE'
END

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = N'DropWorkflowInbox')
BEGIN
	DROP PROCEDURE [DropWorkflowInbox]
	PRINT 'DropWorkflowInbox DROP PROCEDURE'
END

IF EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowApprovalHistory')
BEGIN
	DROP TABLE [WorkflowInbox]
	PRINT 'WorkflowApprovalHistory DROP TABLE'
END

IF  EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessTimer')
BEGIN
	DROP TABLE [WorkflowProcessTimer]
	PRINT 'WorkflowProcessTimer DROP TABLE'
END

IF  EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowProcessAssignment')
BEGIN
DROP TABLE [WorkflowProcessAssignment]
    PRINT 'WorkflowProcessAssignment DROP TABLE'
END

IF  EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowGlobalParameter')
BEGIN
	DROP TABLE WorkflowGlobalParameter
	PRINT 'WorkflowGlobalParameter DROP TABLE'
END

IF  EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowRuntime')
BEGIN
	DROP TABLE WorkflowRuntime
	PRINT 'WorkflowRuntime DROP TABLE'
END

IF  EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'WorkflowSync')
BEGIN
	DROP TABLE WorkflowSync
	PRINT 'WorkflowSync DROP TABLE'
END

COMMIT TRANSACTION