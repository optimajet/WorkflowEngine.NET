DROP PROCEDURE IF EXISTS AddCalendarNameColumn;

CREATE PROCEDURE AddCalendarNameColumn()
BEGIN
    IF NOT EXISTS(SELECT 1
                  FROM INFORMATION_SCHEMA.COLUMNS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = 'workflowprocessinstance'
                    AND COLUMN_NAME = 'CalendarName') THEN
        ALTER TABLE `workflowprocessinstance`
            ADD COLUMN `CalendarName` varchar(256) null;
    END IF;
END;

CALL AddCalendarNameColumn();

DROP PROCEDURE IF EXISTS AddCalendarNameColumn;

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
                SELECT 25, CURRENT_TIMESTAMP, ''Migration25AlterColumn_WorkflowProcessInstance_CalendarName''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 25
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
