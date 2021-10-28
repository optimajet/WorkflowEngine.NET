ALTER TABLE "WorkflowProcessInstance" ADD COLUMN "SubprocessName" text NULL;

UPDATE "WorkflowProcessInstance" SET "SubprocessName" = "StartingTransition";
