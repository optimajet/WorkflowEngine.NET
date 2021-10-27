ALTER TABLE "WorkflowProcessTransitionHistory" ADD COLUMN "ActorName" character varying(256) NULL;
ALTER TABLE "WorkflowProcessTransitionHistory" ADD COLUMN "ExecutorName" character varying(256) NULL;

CREATE TABLE IF NOT EXISTS "WorkflowProcessAssignment"
(
    "Id" uuid NOT NULL,
    "AssignmentCode" character varying(256) NOT NULL,
    "ProcessId" uuid NOT NULL,
    "Name" character varying(256) NOT NULL,
    "Description" text,
    "StatusState" character varying(256) NOT NULL,
    "DateCreation" timestamp NOT NULL,
    "DateStart" timestamp,
    "DateFinish" timestamp,
    "DeadlineToStart" timestamp,
    "DeadlineToComplete" timestamp,
    "Executor" character varying(256) NOT NULL,
    "Observers" text,
    "Tags" text,
    "IsActive" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "WorkflowProcessAssignment_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessAssignment_ProcessId_idx"  ON "WorkflowProcessAssignment" USING btree ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_AssignmentCode_idx"  ON "WorkflowProcessAssignment" USING btree ("AssignmentCode");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_Executor_idx"  ON "WorkflowProcessAssignment" USING btree ("Executor");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_ProcessId_Executor_idx"  ON "WorkflowProcessAssignment" USING btree ("ProcessId", "Executor");

CREATE OR REPLACE FUNCTION public."DropUnusedWorkflowProcessScheme"()
    RETURNS integer
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE status INTEGER;
BEGIN
    DELETE FROM "WorkflowProcessScheme" AS wps
    WHERE wps."IsObsolete"
      AND NOT EXISTS (SELECT * FROM "WorkflowProcessInstance" wpi WHERE wpi."SchemeId" = wps."Id" );

    SELECT COUNT(*) INTO status
    FROM "WorkflowProcessInstance" wpi LEFT OUTER JOIN "WorkflowProcessScheme" wps ON wpi."SchemeId" = wps."Id"
    WHERE wps."Id" IS NULL;

    RETURN status;
END;
$BODY$;
