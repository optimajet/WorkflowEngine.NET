/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySQL
Version: 4.2
File: update_4_1_to_4_2.sql
*/

ALTER TABLE `workflowprocessinstancestatus`
    ADD `RuntimeId` varchar(450) NULL;

UPDATE `workflowprocessinstancestatus`
SET `RuntimeId` = '00000000-0000-0000-0000-000000000000'
WHERE `RuntimeId` IS NULL;

ALTER TABLE `workflowprocessinstancestatus`
    MODIFY `RuntimeId` varchar(450) NOT NULL;

set @exist := (select count(*)
               from information_schema.statistics
               where table_name = 'workflowprocessinstancestatus'
                 and index_name = 'IX_WorkflowProcessInstanceStatus_Status'
                 and table_schema = database());
set @sqlstmt := if(@exist <= 0,
                   'CREATE INDEX `IX_WorkflowProcessInstanceStatus_Status` ON `workflowprocessinstancestatus` (`Status`)',
                   'select ''INFO: Index already exists.''');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;

set @exist := (select count(*)
               from information_schema.statistics
               where table_name = 'workflowprocessinstancestatus'
                 and index_name = 'IX_WorkflowProcessInstanceStatus_Status_Runtime'
                 and table_schema = database());
set @sqlstmt := if(@exist <= 0,
                   'CREATE INDEX `IX_WorkflowProcessInstanceStatus_Status_Runtime` ON `workflowprocessinstancestatus` (`Status`, `RuntimeId`)',
                   'select ''INFO: Index already exists.''');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;

ALTER TABLE `workflowprocessinstancestatus`
    ADD `SetTime` datetime NULL;

UPDATE `workflowprocessinstancestatus`
SET `SetTime` = curdate()
WHERE `SetTime` IS NULL;

ALTER TABLE `workflowprocessinstancestatus`
    MODIFY `SetTime` datetime NOT NULL;

CREATE TABLE IF NOT EXISTS `workflowruntime`
(
    `RuntimeId`            varchar(450) NOT NULL,
    `Lock`                 binary(16)   NOT NULL,
    `Status`               TINYINT(4)   NOT NULL,
    `RestorerId`           varchar(450),
    `NextTimerTime`        datetime(3),
    `NextServiceTimerTime` datetime(3),
    `LastAliveSignal`      datetime(3),
    PRIMARY KEY (`RuntimeId`)
);

CREATE TABLE IF NOT EXISTS `workflowsync`
(
    `Name` varchar(450) NOT NULL,
    `Lock` binary(16)   NOT NULL,
    PRIMARY KEY (`Name`)
);

INSERT IGNORE `workflowruntime` (`RuntimeId`, `Lock`, `Status`)
VALUES ('00000000-0000-0000-0000-000000000000', UUID_TO_BIN(UUID()), 100);

INSERT IGNORE INTO `workflowsync`(`Name`, `Lock`)
VALUES ('Timer', UUID_TO_BIN(UUID()));
INSERT IGNORE INTO `workflowsync`(`Name`, `Lock`)
VALUES ('ServiceTimer', UUID_TO_BIN(UUID()));

ALTER TABLE `workflowprocesstimer`
    ADD `RootProcessId` binary(16) NULL;

UPDATE `workflowprocesstimer`
SET `RootProcessId` = (SELECT `RootProcessId` FROM `workflowprocessinstance` wpi WHERE wpi.`Id` = `ProcessId`);

ALTER TABLE `workflowprocesstimer`
    MODIFY `RootProcessId` binary(16) NOT NULL;

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

set @exist := (select count(*) from information_schema.statistics where table_name = 'workflowprocessinstance'
                                                                        and index_name = 'ix_workflowprocessinstance_rootprocessid' and table_schema = database());
set @sqlstmt := if( @exist <= 0, 'CREATE INDEX `ix_workflowprocessinstance_rootprocessid` ON `workflowprocessinstance` (`RootProcessId`)',
                    'select ''INFO: Index already exists.''');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;

ALTER TABLE `WorkflowProcessInstance` ADD `StartingTransition`  longtext NULL;

UPDATE `WorkflowProcessInstance` SET  `StartingTransition` = (SELECT  `StartingTransition` FROM `WorkflowProcessScheme` wps WHERE wps.`Id` = `SchemeId`);

ALTER TABLE `workflowprocesstimer` MODIFY `NextExecutionDateTime` datetime(3) NOT NULL;