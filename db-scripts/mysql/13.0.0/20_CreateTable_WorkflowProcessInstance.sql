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
    `CreationDate`  datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastTransitionDate` datetime NULL,
    `CalendarName` varchar(256) null,
    PRIMARY KEY  (`Id`),
    KEY `SchemeId` (`SchemeId`)
);

-- DbScriptPublisher: mark migration as applied
SET @sql = (
    SELECT IF(
               EXISTS (
                   SELECT 1
                   FROM information_schema.tables
                   WHERE table_schema = DATABASE()
                     AND table_name = 'VersionInfo'
                     AND table_type = 'BASE TABLE'
               ),
               'INSERT INTO `VersionInfo` (`Version`, `AppliedOn`, `Description`)
                SELECT 20, CURRENT_TIMESTAMP, ''Migration20CreateTable_WorkflowProcessInstance''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 20
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
