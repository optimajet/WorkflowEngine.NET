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
                SELECT 90, CURRENT_TIMESTAMP, ''Migration90CreateTable_WorkflowProcessScheme''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 90
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
