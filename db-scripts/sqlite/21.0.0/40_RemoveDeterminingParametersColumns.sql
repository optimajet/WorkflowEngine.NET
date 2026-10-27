/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for SQLite
Version: 21.0
File: RemoveDeterminingParametersColumns.sql
Description: Removes deprecated determining parameters columns.
Note: Requires SQLite 3.35.0+ for ALTER TABLE DROP COLUMN support.
*/

-- Drop index that uses DefiningParametersHash
DROP INDEX IF EXISTS "WorkflowProcessScheme_DefiningParametersHash_idx";

-- Drop columns from WorkflowProcessScheme
ALTER TABLE "WorkflowProcessScheme" DROP COLUMN "DefiningParametersHash";
ALTER TABLE "WorkflowProcessScheme" DROP COLUMN "DefiningParameters";

-- Drop column from WorkflowProcessInstance
ALTER TABLE "WorkflowProcessInstance" DROP COLUMN "IsDeterminingParametersChanged";

-- DbScriptPublisher: mark migration as applied
-- !!! Remove this block if your database does not contain the "VersionInfo" table created by FluentMigrator. !!!
INSERT INTO "VersionInfo" ("Version", "AppliedOn", "Description")
SELECT 40, CURRENT_TIMESTAMP, 'Migration40RemoveDeterminingParametersColumns'
WHERE NOT EXISTS (
    SELECT 1
    FROM "VersionInfo"
    WHERE "Version" = 40
);
