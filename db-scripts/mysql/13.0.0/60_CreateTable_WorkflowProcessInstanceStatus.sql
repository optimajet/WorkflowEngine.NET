CREATE TABLE IF NOT EXISTS `workflowprocessinstancestatus` (
    `Id` binary(16) NOT NULL,
    `Status` tinyint(4) NOT NULL,
    `Lock` binary(16) NOT NULL,
    `RuntimeId` varchar (450) NOT NULL,
    `SetTime` datetime NOT NULL,
    PRIMARY KEY  (`Id`),
    KEY `Status` (`Status`)
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
                SELECT 60, CURRENT_TIMESTAMP, ''Migration60CreateTable_WorkflowProcessInstanceStatus''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 60
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
