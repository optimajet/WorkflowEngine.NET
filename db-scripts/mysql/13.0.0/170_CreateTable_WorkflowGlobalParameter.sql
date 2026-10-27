CREATE TABLE IF NOT EXISTS `workflowglobalparameter` (
    `Id` binary(16) NOT NULL,
    `Type` varchar(512)  NOT NULL,
    `Name` varchar(256) NOT NULL,
    `Value` longtext NOT NULL,
    PRIMARY KEY  (`Id`)
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
                SELECT 170, CURRENT_TIMESTAMP, ''Migration170CreateTable_WorkflowGlobalParameter''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 170
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
