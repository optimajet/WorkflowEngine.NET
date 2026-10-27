CREATE UNIQUE INDEX `ix_workflowprocessinstancepersistence_processId_parameterName` ON `workflowprocessinstancepersistence` (`ProcessId`, `ParameterName`);

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
                SELECT 250, CURRENT_TIMESTAMP, ''Migration250CreateIndex_WorkflowProcessInstancePersistence_ProcessId_ParameterName''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 250
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
