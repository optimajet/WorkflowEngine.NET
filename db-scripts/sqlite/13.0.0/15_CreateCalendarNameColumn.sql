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

-- DbScriptPublisher: mark migration as applied
-- !!! Remove this block if your database does not contain the "VersionInfo" table created by FluentMigrator. !!!
INSERT INTO "VersionInfo" ("Version", "AppliedOn", "Description")
SELECT 15, CURRENT_TIMESTAMP, 'Migration15CreateCalendarNameColumn'
WHERE NOT EXISTS (
    SELECT 1
    FROM "VersionInfo"
    WHERE "Version" = 15
);
