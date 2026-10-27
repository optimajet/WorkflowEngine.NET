CREATE TABLE IF NOT EXISTS `workflowinbox` (
    `Id` binary(16) NOT NULL,
    `ProcessId` binary(16) default NULL,
    `IdentityId` varchar(256) default NULL,
    `AddingDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `AvailableCommands` varchar(1024) DEFAULT '',
    PRIMARY KEY  (`Id`),
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
                SELECT 10, CURRENT_TIMESTAMP, ''Migration10CreateTable_WorkflowInbox''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 10
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
