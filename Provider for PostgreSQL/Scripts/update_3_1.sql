/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 3.1
File: update_3_1.sql
*/

ALTER TABLE "WorkflowInbox" ALTER COLUMN "IdentityId" TYPE character varying(256);

ALTER TABLE "WorkflowGlobalParameter" ALTER COLUMN "Type" TYPE character varying(512);

CREATE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Type_idx"  ON "WorkflowGlobalParameter" USING btree ("Type");

CREATE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Name_idx"  ON "WorkflowGlobalParameter" USING btree ("Name");

DROP INDEX IF EXISTS "WorkflowProcessInstance_SchemeId_idx";

ALTER TABLE "WorkflowProcessScheme" ALTER COLUMN "DefiningParametersHash" TYPE character varying(24);