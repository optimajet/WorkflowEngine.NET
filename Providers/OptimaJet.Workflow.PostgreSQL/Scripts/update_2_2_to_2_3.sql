/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 2.3
File: update_2_2_to_2_3.sql
*/

ALTER TABLE "WorkflowInbox" ALTER COLUMN "IdentityId" TYPE uuid;
 
