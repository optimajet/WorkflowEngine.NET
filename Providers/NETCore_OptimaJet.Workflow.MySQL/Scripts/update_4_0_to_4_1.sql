/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySQL
Version: 4.1
File: update_4_0_to_4_1.sql
*/

ALTER TABLE `WorkflowProcessInstance` ADD `TenantId` varchar(1024) NULL
ALTER TABLE `workflowscheme` ADD `Tags` varchar(1024) NULL