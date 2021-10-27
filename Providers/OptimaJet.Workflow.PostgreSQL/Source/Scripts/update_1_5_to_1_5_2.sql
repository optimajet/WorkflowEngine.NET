/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 1.5.2
File: update_1_5_to_1_5_2.sql
*/

ALTER TABLE "WorkflowProcessTimer" ALTER COLUMN "NextExecutionDateTime" TYPE timestamp;
ALTER TABLE "WorkflowProcessTransitionHistory" ALTER COLUMN "TransitionTime" TYPE timestamp;
 
