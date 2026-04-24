/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 21.0
File: RemoveConstraintsFromDeterminingParametersColumns.sql
Description: Removes constraints and indexes from deprecated determining parameters columns.
             Columns are kept for backward compatibility until next release.
*/

-- Drop index that uses DefiningParametersHash
DROP INDEX IF EXISTS "WorkflowProcessScheme_DefiningParametersHash_idx";

-- Remove NOT NULL constraint from DefiningParametersHash
ALTER TABLE "WorkflowProcessScheme" ALTER COLUMN "DefiningParametersHash" DROP NOT NULL;

-- Remove NOT NULL constraint from DefiningParameters
ALTER TABLE "WorkflowProcessScheme" ALTER COLUMN "DefiningParameters" DROP NOT NULL;

-- Remove NOT NULL constraint from IsDeterminingParametersChanged
ALTER TABLE "WorkflowProcessInstance" ALTER COLUMN "IsDeterminingParametersChanged" DROP NOT NULL;
