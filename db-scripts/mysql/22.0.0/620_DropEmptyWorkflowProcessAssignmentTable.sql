DROP PROCEDURE IF EXISTS `workflow_drop_empty_assignment_table`;

CREATE PROCEDURE `workflow_drop_empty_assignment_table`()
BEGIN
    DECLARE assignment_table_exists BOOLEAN DEFAULT FALSE;
    DECLARE assignment_table_has_rows BOOLEAN DEFAULT FALSE;

    SELECT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = DATABASE()
          AND table_name = 'workflowprocessassignment'
    ) INTO assignment_table_exists;

    IF assignment_table_exists THEN
        SELECT EXISTS (SELECT 1 FROM `workflowprocessassignment`)
        INTO assignment_table_has_rows;

        IF NOT assignment_table_has_rows THEN
            DROP TABLE `workflowprocessassignment`;
        END IF;
    END IF;
END;

CALL `workflow_drop_empty_assignment_table`();

DROP PROCEDURE `workflow_drop_empty_assignment_table`;

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
                SELECT 620, CURRENT_TIMESTAMP, ''Migration620DropEmptyWorkflowProcessAssignmentTable''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 620
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
