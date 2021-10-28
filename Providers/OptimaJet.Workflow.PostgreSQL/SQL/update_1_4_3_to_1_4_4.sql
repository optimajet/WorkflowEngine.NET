/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 1.4.4
File: update_1_4_3_to_1_4_4.sql
*/

CREATE TABLE IF NOT EXISTS "WorkflowGlobalParameter" (
  "Id" uuid NOT NULL,
  "Type" character varying(256) NOT NULL,
  "Name" character varying(256) NOT NULL,
  "Value" text NOT NULL,
   CONSTRAINT "WorkflowGlobalParameter_pkey" PRIMARY KEY ("Id")
);
