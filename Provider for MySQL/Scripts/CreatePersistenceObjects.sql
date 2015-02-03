/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySQL
Version: 1.4.3
File: CreatePersistenceObjects.sql
*/

-- Dumping structure for table workflowinbox
CREATE TABLE IF NOT EXISTS `workflowinbox` (
  `Id` binary(16) NOT NULL,
  `ProcessId` binary(16) default NULL,
  `IdentityId` varchar(256) default NULL,
  PRIMARY KEY  (`Id`),
  KEY `ProcessId` (`ProcessId`),
  KEY `IdentityId` (`IdentityId`)
);




-- Dumping structure for table workflowprocessinstance
CREATE TABLE IF NOT EXISTS `workflowprocessinstance` (
  `Id` binary(16) NOT NULL,
  `StateName` varchar(256) default NULL,
  `ActivityName` varchar(256) NOT NULL,
  `SchemeId` binary(16) NOT NULL,
  `PreviousState` varchar(256) default NULL,
  `PreviousStateForDirect` varchar(256) default NULL,
  `PreviousStateForReverse` varchar(256) default NULL,
  `PreviousActivity` varchar(256) default NULL,
  `PreviousActivityForDirect` varchar(256) default NULL,
  `PreviousActivityForReverse` varchar(256) default NULL,
  `IsDeterminingParametersChanged` bit(1) NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `SchemeId` (`SchemeId`)
);




-- Dumping structure for table workflowprocessinstancepersistence
CREATE TABLE IF NOT EXISTS `workflowprocessinstancepersistence` (
  `Id` binary(16) NOT NULL,
  `ProcessId` binary(16) NOT NULL,
  `ParameterName` varchar(256) NOT NULL,
  `Value` longtext NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `ProcessId` (`ProcessId`)
);




-- Dumping structure for table workflowprocessinstancestatus
CREATE TABLE IF NOT EXISTS `workflowprocessinstancestatus` (
  `Id` binary(16) NOT NULL,
  `Status` tinyint(4) NOT NULL,
  `Lock` binary(16) NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `Status` (`Status`)
);




-- Dumping structure for table workflowprocessscheme
CREATE TABLE IF NOT EXISTS `workflowprocessscheme` (
  `Id` binary(16) NOT NULL,
  `Scheme` longtext NOT NULL,
  `DefiningParameters` longtext NOT NULL,
  `DefiningParametersHash` varchar(1024) NOT NULL,
  `SchemeCode` varchar(256) NOT NULL,
  `IsObsolete` bit(1) NOT NULL,
  PRIMARY KEY  (`Id`)
);




-- Dumping structure for table workflowprocesstimer
CREATE TABLE IF NOT EXISTS `workflowprocesstimer` (
  `Id` binary(16) NOT NULL,
  `ProcessId` binary(16) NOT NULL,
  `Name` varchar(256) NOT NULL,
  `NextExecutionDateTime` datetime NOT NULL,
  `Ignore` bit(1) NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `ProcessId` (`ProcessId`),
  KEY `Name` (`Name`),
  KEY `NextExecutionDateTime` (`NextExecutionDateTime`)
);




-- Dumping structure for table workflowprocesstransitionhistory
CREATE TABLE IF NOT EXISTS `workflowprocesstransitionhistory` (
  `Id` binary(16) NOT NULL,
  `ProcessId` binary(16) NOT NULL,
  `ExecutorIdentityId` varchar(256) default NULL,
  `ActorIdentityId` varchar(256) default NULL,
  `FromActivityName` varchar(256) NOT NULL,
  `ToActivityName` varchar(256) NOT NULL,
  `ToStateName` varchar(256) default NULL,
  `TransitionTime` datetime NOT NULL,
  `TransitionClassifier` varchar(256) NOT NULL,
  `FromStateName` varchar(256) default NULL,
  `TriggerName` varchar(256) default NULL,
  `IsFinalised` bit(1) NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `ProcessId` (`ProcessId`),
  KEY `ExecutorIdentityId` (`ExecutorIdentityId`),
  KEY `ActorIdentityId` (`ActorIdentityId`)
);




-- Dumping structure for table workflowscheme
CREATE TABLE IF NOT EXISTS `workflowscheme` (
  `Code` varchar(256) NOT NULL,
  `Scheme` longtext NOT NULL,
  PRIMARY KEY  (`Code`)
);
