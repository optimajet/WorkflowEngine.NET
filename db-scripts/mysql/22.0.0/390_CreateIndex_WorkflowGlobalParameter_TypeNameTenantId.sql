ALTER TABLE `workflowglobalparameter`
    ADD COLUMN `TenantIdForUniqueIndex` varchar(128)
        GENERATED ALWAYS AS (IFNULL(`TenantId`, CHAR(0))) STORED,
    ADD UNIQUE INDEX `ix_workflowglobalparameter_type_name_tenantid` (`Type`, `Name`, `TenantIdForUniqueIndex`);

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
                SELECT 390, CURRENT_TIMESTAMP, ''Migration390CreateIndex_WorkflowGlobalParameter_TypeNameTenantId''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 390
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
