/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 4.0
File: update_4_0.sql
*/

ALTER TABLE "WorkflowScheme" ADD COLUMN "CanBeInlined" boolean NOT NULL DEFAULT FALSE;
ALTER TABLE "WorkflowScheme" ADD COLUMN "InlinedSchemes" character varying(1024) NULL;

