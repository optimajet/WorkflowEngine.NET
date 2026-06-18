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
