ALTER TABLE `workflowprocessinstancestatus`
    ADD COLUMN `TenantId` varchar(128) NULL;

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
                SELECT 550, CURRENT_TIMESTAMP, ''Migration550AddColumn_WorkflowProcessInstanceStatus_TenantId''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 550
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
