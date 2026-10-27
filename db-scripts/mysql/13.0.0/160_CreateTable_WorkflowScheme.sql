CREATE TABLE IF NOT EXISTS `workflowscheme` (
    `Code` varchar(128) NOT NULL,
    `Scheme` longtext NOT NULL,
    `CanBeInlined` bit(1) NOT NULL default 0,
    `InlinedSchemes` varchar(1024) NULL,
    `Tags` varchar(1024) NULL,
    PRIMARY KEY  (`Code`)
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
                SELECT 160, CURRENT_TIMESTAMP, ''Migration160CreateTable_WorkflowScheme''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 160
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
