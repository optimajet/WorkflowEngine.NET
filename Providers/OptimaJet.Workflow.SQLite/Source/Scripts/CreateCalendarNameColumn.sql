PRAGMA writable_schema = ON;

UPDATE sqlite_schema
SET sql = CASE
              WHEN instr(sql, ', CONSTRAINT ') > 0 THEN
                  replace(sql, ', CONSTRAINT ', ', "CalendarName" TEXT NULL, CONSTRAINT ')
              ELSE
                  substr(rtrim(sql), 1, length(rtrim(sql)) - 1) || ', "CalendarName" TEXT NULL)'
          END
WHERE type = 'table'
  AND name = 'WorkflowProcessInstance'
  AND instr(sql, '"CalendarName"') = 0;

PRAGMA writable_schema = OFF;

CREATE INDEX IF NOT EXISTS "WorkflowProcessInstance_CalendarName_idx" ON "WorkflowProcessInstance" ("CalendarName");
