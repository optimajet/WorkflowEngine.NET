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
                SELECT 110, CURRENT_TIMESTAMP, ''Migration110CreateTable_WorkflowProcessTimer''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 110
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
