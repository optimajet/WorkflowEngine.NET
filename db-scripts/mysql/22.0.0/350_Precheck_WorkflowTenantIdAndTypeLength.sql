DROP PROCEDURE IF EXISTS `workflow_precheck_tenant_schema`;

CREATE PROCEDURE `workflow_precheck_tenant_schema`()
BEGIN
    IF EXISTS (
        SELECT 1
        FROM `workflowglobalparameter`
        WHERE CHAR_LENGTH(`Type`) > 128
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'BREAKING CHANGES DETECTED: WorkflowGlobalParameter.Type exceeds 128 characters. Contact support@optimajet.com.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM `workflowprocessinstance`
        WHERE CHAR_LENGTH(`TenantId`) > 128
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'BREAKING CHANGES DETECTED: WorkflowProcessInstance.TenantId exceeds 128 characters. Contact support@optimajet.com.';
    END IF;
END;

CALL `workflow_precheck_tenant_schema`();

DROP PROCEDURE `workflow_precheck_tenant_schema`;

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
                SELECT 350, CURRENT_TIMESTAMP, ''Migration350Precheck_WorkflowTenantIdAndTypeLength''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 350
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
