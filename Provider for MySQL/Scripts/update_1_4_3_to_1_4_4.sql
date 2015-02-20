/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySQL
Version: 1.4.4
File: update_1_4_3_to_1_4_4.sql
*/

-- Dumping structure for table workflowglobalparameter
CREATE TABLE IF NOT EXISTS `workflowglobalparameter` (
  `Id` binary(16) NOT NULL,
  `Type` varchar(256)  NOT NULL,
  `Name` varchar(256) NOT NULL,
  `Value` longtext NOT NULL,
  PRIMARY KEY  (`Id`)
);
