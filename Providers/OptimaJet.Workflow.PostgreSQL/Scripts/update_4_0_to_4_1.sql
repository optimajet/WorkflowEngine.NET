/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 4.1
File: update_4_0_to_4_1.sql
*/

ALTER TABLE "WorkflowProcessInstance" ADD COLUMN "TenantId" character varying(1024) NULL
ALTER TABLE "WorkflowScheme" ADD COLUMN "Tags" character varying(1024) NULL