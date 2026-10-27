INSERT IGNORE INTO `workflowsync`(`Name`,`Lock`) VALUES ('Timer', UUID_TO_BIN(UUID()));
INSERT IGNORE INTO `workflowsync`(`Name`,`Lock`) VALUES ('ServiceTimer', UUID_TO_BIN(UUID()));

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
                SELECT 210, CURRENT_TIMESTAMP, ''Migration210InsertIgnore_WorkflowSync''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 210
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
