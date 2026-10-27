CREATE INDEX `ix_wfps_schemecode_tenantid_rootschemeid_isobsolete`
    ON `workflowprocessscheme` (`SchemeCode`, `TenantId`, `RootSchemeId`, `IsObsolete`);

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
                SELECT 600, CURRENT_TIMESTAMP, ''Migration600CreateIndex_WorkflowProcessScheme_SchemeTenantRootIsObsolete''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 600
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
