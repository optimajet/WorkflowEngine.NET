/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySQL
Version: 4.0
File: update_4_0.sql
*/

ALTER TABLE `workflowscheme` ADD `CanBeInlined` bit(1)  NOT NULL default 0
ALTER TABLE `workflowscheme` ADD `InlinedSchemes` varchar(1024) NULL