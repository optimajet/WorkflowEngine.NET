/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 1.5
File: update_1_4_4_to_1_5.sql
*/

ALTER TABLE "WorkflowProcessInstance" ADD COLUMN "ParentProcessId" uuid NULL

ALTER TABLE "WorkflowProcessInstance" ADD COLUMN "RootProcessId" uuid NULL

UPDATE "WorkflowProcessInstance" SET "RootProcessId" = "Id" WHERE "RootProcessId" IS NULL 

ALTER TABLE "WorkflowProcessInstance" ALTER COLUMN "RootProcessId" SET NOT NULL

ALTER TABLE "WorkflowProcessScheme" ADD COLUMN "RootSchemeCode" character varying(256) NULL

ALTER TABLE "WorkflowProcessScheme" ADD COLUMN "RootSchemeId" uuid NULL

ALTER TABLE "WorkflowProcessScheme" ADD COLUMN "AllowedActivities" text NULL

ALTER TABLE "WorkflowProcessScheme" ADD COLUMN "StartingTransition" text NULL