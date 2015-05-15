/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySQL
Version: 1.5
File: update_1_4_4_to_1_5.sql
*/

ALTER TABLE `WorkflowProcessInstance` ADD `ParentProcessId` binary(16) NULL

ALTER TABLE `WorkflowProcessInstance` ADD `RootProcessId` binary(16) NULL

UPDATE `WorkflowProcessInstance` SET `RootProcessId` = `Id` WHERE `RootProcessId` IS NULL 

ALTER TABLE `WorkflowProcessInstance` MODIFY COLUMN `RootProcessId` binary(16) NOT NULL

ALTER TABLE `WorkflowProcessScheme` ADD `RootSchemeCode` varchar(256) NULL

ALTER TABLE `WorkflowProcessScheme` ADD `RootSchemeId` binary(16) NULL

ALTER TABLE `WorkflowProcessScheme` ADD `AllowedActivities` longtext NULL

ALTER TABLE `WorkflowProcessScheme` ADD `StartingTransition` longtext NULL