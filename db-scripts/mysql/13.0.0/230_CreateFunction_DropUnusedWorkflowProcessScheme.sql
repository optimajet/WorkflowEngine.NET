DROP FUNCTION IF EXISTS `DropUnusedWorkflowProcessScheme`;

CREATE FUNCTION `DropUnusedWorkflowProcessScheme`()
    RETURNS INTEGER
    DETERMINISTIC
BEGIN
    DECLARE st INTEGER;

DELETE FROM `workflowprocessscheme`
WHERE `IsObsolete` = 1
  AND NOT EXISTS (
    SELECT 1 FROM `workflowprocessinstance`
    WHERE `workflowprocessinstance`.`SchemeId` = `workflowprocessscheme`.`Id`
);

SELECT COUNT(*) INTO st
FROM `workflowprocessinstance`
         LEFT JOIN `workflowprocessscheme` ON `workflowprocessinstance`.`SchemeId` = `workflowprocessscheme`.`Id`
WHERE `workflowprocessscheme`.`Id` IS NULL;

RETURN st;
END;

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
                SELECT 230, CURRENT_TIMESTAMP, ''Migration230CreateFunction_DropUnusedWorkflowProcessScheme''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 230
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
