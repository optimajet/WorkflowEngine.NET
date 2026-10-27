CREATE INDEX `ix_workflowprocessscheme_schemecode_hash_isobsolete` ON `workflowprocessscheme` (`SchemeCode`,`DefiningParametersHash`,`IsObsolete`);

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
                SELECT 100, CURRENT_TIMESTAMP, ''Migration100CreateIndex_WorkflowProcessScheme_SchemeCode''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 100
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
