-- noinspection SqlNoDataSourceInspectionForFile

/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySQL
Version: 5.0
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
  `TenantId` varchar(1024) NULL,
  `StartingTransition` longtext NULL,
  `SubprocessName` longtext NULL,
  PRIMARY KEY  (`Id`),
  KEY `SchemeId` (`SchemeId`)
);

CREATE INDEX `ix_workflowprocessinstance_rootprocessid` ON `workflowprocessinstance` (`RootProcessId`);

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
  `RuntimeId` varchar (450) NOT NULL,
  `SetTime` datetime NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `Status` (`Status`)
);

CREATE INDEX `IX_WorkflowProcessInstanceStatus_Status` ON `workflowprocessinstancestatus` (`Status`);
CREATE INDEX `IX_WorkflowProcessInstanceStatus_Status_Runtime` ON `workflowprocessinstancestatus` (`Status`, `RuntimeId`);

CREATE TABLE IF NOT EXISTS `workflowprocessscheme` (
  `Id` binary(16) NOT NULL,
  `Scheme` longtext NOT NULL,
  `DefiningParameters` longtext NOT NULL,
  `DefiningParametersHash` varchar(24) NOT NULL,
  `SchemeCode` varchar(256) NOT NULL,
  `IsObsolete` bit(1) NOT NULL,
  `RootSchemeCode` varchar(256) NULL,
  `RootSchemeId` binary(16) NULL,
  `AllowedActivities` longtext NULL,
  `StartingTransition` longtext NULL,
  PRIMARY KEY  (`Id`)
);

CREATE INDEX `ix_workflowprocessscheme_schemecode_hash_isobsolete` ON `workflowprocessscheme` (`SchemeCode`,`DefiningParametersHash`,`IsObsolete`);

CREATE TABLE IF NOT EXISTS `workflowprocesstimer` (
  `Id` binary(16) NOT NULL,
  `ProcessId` binary(16) NOT NULL,
  `RootProcessId` binary(16) NOT NULL,
  `Name` varchar(256) NOT NULL,
  `NextExecutionDateTime` datetime(3) NOT NULL,
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
  `CanBeInlined` bit(1) NOT NULL default 0,
  `InlinedSchemes` varchar(1024) NULL,
  `Tags` varchar(1024) NULL,
  PRIMARY KEY  (`Code`)
);

CREATE TABLE IF NOT EXISTS `workflowglobalparameter` (
  `Id` binary(16) NOT NULL,
  `Type` varchar(512)  NOT NULL,
  `Name` varchar(256) NOT NULL,
  `Value` longtext NOT NULL,
  PRIMARY KEY  (`Id`)
);

CREATE INDEX `ix_workflowglobalparameter_type_name` ON `workflowglobalparameter` (`Type`,`Name`);

CREATE TABLE IF NOT EXISTS `workflowruntime` (
	`RuntimeId` varchar(450) NOT NULL
	,`Lock` binary(16) NOT NULL
	,`Status` TINYINT(4) NOT NULL
	,`RestorerId` varchar(450)
	,`NextTimerTime` datetime(3)
	,`NextServiceTimerTime` datetime(3)
	,`LastAliveSignal` datetime(3),
    PRIMARY KEY (`RuntimeId`)
);

CREATE TABLE IF NOT EXISTS `workflowsync` (
	`Name` varchar(450) NOT NULL
	,`Lock` binary(16) NOT NULL
    , PRIMARY KEY (`Name`)
);

INSERT IGNORE INTO `workflowsync`(`Name`,`Lock`) VALUES ('Timer', UUID_TO_BIN(UUID()));
INSERT IGNORE INTO `workflowsync`(`Name`,`Lock`) VALUES ('ServiceTimer', UUID_TO_BIN(UUID()));

CREATE TABLE IF NOT EXISTS `workflowapprovalhistory`
(
    `Id`               binary(16)    NOT NULL,
    `ProcessId`        binary(16)    NOT NULL,
    `IdentityId`       varchar(256)  NULL,
    `AllowedTo`        longtext      NULL,
    `TransitionTime`   datetime      NULL,
    `Sort`             bigint        NULL,
    `InitialState`     varchar(1024) NOT NULL,
    `DestinationState` varchar(1024) NOT NULL,
    `TriggerName`      varchar(1024) NULL,
    `Commentary`       longtext      NULL,
    PRIMARY KEY (`Id`),
    KEY `ProcessId` (`ProcessId`)
);

