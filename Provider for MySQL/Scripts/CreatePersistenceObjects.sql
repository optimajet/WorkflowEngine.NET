/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySQL
Version: 2.3
File: CreatePersistenceObjects.sql
*/

CREATE TABLE IF NOT EXISTS `workflowinbox` (
  `Id` binary(16) NOT NULL,
  `ProcessId` binary(16) default NULL,
  `IdentityId` varchar(256) default NULL,
  PRIMARY KEY  (`Id`),
  KEY `ProcessId` (`ProcessId`),
  KEY `IdentityId` (`IdentityId`)
);

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
  `ParentProcessId` binary(16) NULL,
  `RootProcessId` binary(16) NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `SchemeId` (`SchemeId`)
);

CREATE TABLE IF NOT EXISTS `workflowprocessinstancepersistence` (
  `Id` binary(16) NOT NULL,
  `ProcessId` binary(16) NOT NULL,
  `ParameterName` varchar(256) NOT NULL,
  `Value` longtext NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `ProcessId` (`ProcessId`)
);

CREATE TABLE IF NOT EXISTS `workflowprocessinstancestatus` (
  `Id` binary(16) NOT NULL,
  `Status` tinyint(4) NOT NULL,
  `Lock` binary(16) NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `Status` (`Status`)
);

CREATE TABLE IF NOT EXISTS `workflowprocessscheme` (
  `Id` binary(16) NOT NULL,
  `Scheme` longtext NOT NULL,
  `DefiningParameters` longtext NOT NULL,
  `DefiningParametersHash` varchar(1024) NOT NULL,
  `SchemeCode` varchar(256) NOT NULL,
  `IsObsolete` bit(1) NOT NULL,
  `RootSchemeCode` varchar(256) NULL,
  `RootSchemeId` binary(16) NULL,
  `AllowedActivities` longtext NULL,
  `StartingTransition` longtext NULL,
  PRIMARY KEY  (`Id`)
);

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

CREATE TABLE IF NOT EXISTS `workflowscheme` (
  `Code` varchar(128) NOT NULL,
  `Scheme` longtext NOT NULL,
  PRIMARY KEY  (`Code`)
);

CREATE TABLE IF NOT EXISTS `workflowglobalparameter` (
  `Id` binary(16) NOT NULL,
  `Type` varchar(256)  NOT NULL,
  `Name` varchar(256) NOT NULL,
  `Value` longtext NOT NULL,
  PRIMARY KEY  (`Id`)
);

