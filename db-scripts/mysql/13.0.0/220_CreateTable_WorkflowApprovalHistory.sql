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
    KEY `ProcessId` (`ProcessId`),
    KEY `IdentityId` (`IdentityId`)
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
                SELECT 220, CURRENT_TIMESTAMP, ''Migration220CreateTable_WorkflowApprovalHistory''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 220
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
