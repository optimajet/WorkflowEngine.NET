CREATE TABLE IF NOT EXISTS `workflowprocessinstancepersistence` (
    `Id` binary(16) NOT NULL,
    `ProcessId` binary(16) NOT NULL,
    `ParameterName` varchar(256) NOT NULL,
    `Value` longtext NOT NULL,
    PRIMARY KEY  (`Id`),
    KEY `ProcessId` (`ProcessId`)
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
                SELECT 50, CURRENT_TIMESTAMP, ''Migration50CreateTable_WorkflowProcessInstancePersistence''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 50
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
