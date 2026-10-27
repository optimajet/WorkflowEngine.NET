CREATE INDEX `ix_workflowprocesstimer_processId` ON `workflowprocesstimer` (`ProcessId`);

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
                SELECT 120, CURRENT_TIMESTAMP, ''Migration120CreateIndex_WorkflowProcessTimer_ProcessId''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 120
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
