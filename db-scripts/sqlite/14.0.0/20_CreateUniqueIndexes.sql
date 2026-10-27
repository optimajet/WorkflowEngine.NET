CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowInbox_ProcessId_IdentityId_idx" ON "WorkflowInbox" ("ProcessId", "IdentityId");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowProcessInstancePersistence_ProcessId_ParameterName_idx" ON "WorkflowProcessInstancePersistence" ("ProcessId", "ParameterName");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowProcessTimer_ProcessId_Name_idx" ON "WorkflowProcessTimer" ("ProcessId", "Name");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Type_Name_idx" ON "WorkflowGlobalParameter" ("Type", "Name");

-- DbScriptPublisher: mark migration as applied
-- !!! Remove this block if your database does not contain the "VersionInfo" table created by FluentMigrator. !!!
INSERT INTO "VersionInfo" ("Version", "AppliedOn", "Description")
SELECT 20, CURRENT_TIMESTAMP, 'Migration20CreateUniqueIndexes'
WHERE NOT EXISTS (
    SELECT 1
    FROM "VersionInfo"
    WHERE "Version" = 20
);
