/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 12.2
File: CreatePersistenceObjects.sql
*/
-- WorkflowInbox
CREATE TABLE IF NOT EXISTS "WorkflowInbox"
(
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "IdentityId" character varying(256) NOT NULL,
  "AddingDate" timestamp NOT NULL  DEFAULT localtimestamp,
  "AvailableCommands" character varying(1024) NOT NULL DEFAULT '',
  CONSTRAINT "WorkflowInbox_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowInbox_IdentityId_idx" ON "WorkflowInbox" USING btree ("IdentityId");
CREATE INDEX IF NOT EXISTS "WorkflowInbox_ProcessId_idx"  ON "WorkflowInbox" USING btree ("ProcessId");

--WorkflowProcessInstance
CREATE TABLE IF NOT EXISTS "WorkflowProcessInstance" (
  "Id" uuid NOT NULL,
  "StateName" character varying(256) NULL,
  "ActivityName" character varying(256) NOT NULL,
  "SchemeId" uuid NOT NULL,
  "PreviousState" character varying(256) NULL,
  "PreviousStateForDirect" character varying(256) NULL,
  "PreviousStateForReverse" character varying(256) NULL,
  "PreviousActivity" character varying(256) NULL,
  "PreviousActivityForDirect" character varying(256) NULL,
  "PreviousActivityForReverse" character varying(256) NULL,
  "IsDeterminingParametersChanged" boolean NOT NULL,
  "ParentProcessId" uuid NULL,
  "RootProcessId" uuid NOT NULL,
  "TenantId" character varying(1024) NULL,
  "StartingTransition" text NULL,
  "SubprocessName" text NULL,
  "CreationDate" timestamp NOT NULL DEFAULT localtimestamp,
  "LastTransitionDate" timestamp NULL,
  "CalendarName" character varying(256) null,
  CONSTRAINT "WorkflowProcessInstance_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessInstance_RootProcessId_idx"  ON "WorkflowProcessInstance" USING btree ("RootProcessId");

CREATE INDEX IF NOT EXISTS "IX_CalendarName" ON "WorkflowProcessInstance" USING btree ("CalendarName");

--WorkflowProcessInstancePersistence
CREATE TABLE IF NOT EXISTS "WorkflowProcessInstancePersistence" (
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "ParameterName" character varying(256) NOT NULL,
  "Value" text NOT NULL,
  CONSTRAINT "WorkflowProcessInstancePersistence_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "WorkflowProcessInstancePersistence_ProcessId_idx"  ON "WorkflowProcessInstancePersistence" USING btree ("ProcessId");

--WorkflowProcessInstanceStatus
CREATE TABLE IF NOT EXISTS "WorkflowProcessInstanceStatus" (
  "Id" uuid NOT NULL,
  "Status" smallint NOT NULL,
  "Lock" uuid NOT NULL,
  "RuntimeId" character varying(450) NOT NULL,
  "SetTime" timestamp NOT NULL,
  CONSTRAINT "WorkflowProcessInstanceStatus_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "WorkflowProcessInstanceStatus_Status_idx"  ON "WorkflowProcessInstanceStatus" USING btree ("Status");

CREATE INDEX IF NOT EXISTS "WorkflowProcessInstanceStatus_Status_Runtime_ix"
    ON public."WorkflowProcessInstanceStatus" USING btree
    ("Status" ASC NULLS LAST, "RuntimeId" ASC NULLS LAST)
;

--WorkflowProcessScheme
CREATE TABLE IF NOT EXISTS "WorkflowProcessScheme" (
  "Id" uuid NOT NULL,
  "Scheme" text NOT NULL,
  "DefiningParameters" text NOT NULL,
  "DefiningParametersHash" character varying(24) NOT NULL,
  "SchemeCode" character varying(256) NOT NULL,
  "IsObsolete" boolean NOT NULL,
  "RootSchemeCode" character varying(256) NULL,
  "RootSchemeId" uuid NULL,
  "AllowedActivities" text NULL,
  "StartingTransition" text NULL,
  CONSTRAINT "WorkflowProcessScheme_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "WorkflowProcessScheme_DefiningParametersHash_idx"  ON "WorkflowProcessScheme" USING btree ("DefiningParametersHash");
CREATE INDEX IF NOT EXISTS "WorkflowProcessScheme_SchemeCode_idx"  ON "WorkflowProcessScheme" USING btree ("SchemeCode");
CREATE INDEX IF NOT EXISTS "WorkflowProcessScheme_IsObsolete_idx"  ON "WorkflowProcessScheme" USING btree ("IsObsolete");

--WorkflowProcessTimer
CREATE TABLE IF NOT EXISTS "WorkflowProcessTimer" (
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "RootProcessId" uuid NOT NULL,
  "Name" character varying(256) NOT NULL,
  "NextExecutionDateTime" timestamp NOT NULL,
  "Ignore" boolean NOT NULL,
  CONSTRAINT "WorkflowProcessTimer_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_ProcessId_idx"  ON "WorkflowProcessTimer" USING btree ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_Name_idx"  ON "WorkflowProcessTimer" USING btree ("Name");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_NextExecutionDateTime_idx"  ON "WorkflowProcessTimer" USING btree ("NextExecutionDateTime");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_Ignore_idx"  ON "WorkflowProcessTimer" USING btree ("Ignore");

--WorkflowProcessAssignment
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

--WorkflowProcessTransitionHistory
CREATE TABLE IF NOT EXISTS "WorkflowProcessTransitionHistory" (
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "ExecutorIdentityId" character varying(256) NULL,
  "ActorIdentityId" character varying(256) NULL,
  "ExecutorName" character varying(256) NULL,
  "ActorName" character varying(256) NULL,
  "FromActivityName" character varying(256) NOT NULL,
  "ToActivityName" character varying(256) NOT NULL,
  "ToStateName" character varying(256) NULL,
  "TransitionTime" timestamp NOT NULL,
  "TransitionClassifier" character varying(256) NOT NULL,
  "FromStateName" character varying(256) NULL,
  "TriggerName" character varying(256) NULL,
  "IsFinalised" boolean NOT NULL,
  "StartTransitionTime" timestamp,
  "TransitionDuration" bigint,
  CONSTRAINT "WorkflowProcessTransitionHistory_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ProcessId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ExecutorIdentityId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ExecutorIdentityId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ActorIdentityId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ActorIdentityId");


--WorkflowScheme
CREATE TABLE IF NOT EXISTS "WorkflowScheme" (
  "Code" character varying(256) NOT NULL,
  "Scheme" text NOT NULL,
  "CanBeInlined" boolean NOT NULL DEFAULT FALSE,
  "InlinedSchemes" character varying(1024) NULL,
  "Tags"  character varying(1024) NULL,
  CONSTRAINT "WorkflowScheme_pkey" PRIMARY KEY ("Code")
);

--WorkflowGlobalParameter
CREATE TABLE IF NOT EXISTS "WorkflowGlobalParameter" (
  "Id" uuid NOT NULL,
  "Type" character varying(512) NOT NULL,
  "Name" character varying(256) NOT NULL,
  "Value" text NOT NULL,
   CONSTRAINT "WorkflowGlobalParameter_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Type_idx"  ON "WorkflowGlobalParameter" USING btree ("Type");
CREATE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Name_idx"  ON "WorkflowGlobalParameter" USING btree ("Name");

CREATE TABLE IF NOT EXISTS "WorkflowRuntime" (
  "RuntimeId" character varying(450),
  "Lock" uuid NOT NULL,
  "Status" smallint NOT NULL,
  "RestorerId" character varying(450) NULL,
  "NextTimerTime" timestamp NULL,
  "NextServiceTimerTime" timestamp NULL,
  "LastAliveSignal" timestamp NULL,
  CONSTRAINT "WorkflowRuntime_pkey" PRIMARY KEY ("RuntimeId")
);

CREATE TABLE IF NOT EXISTS "WorkflowSync" (
  "Name" character varying(450),
  "Lock" uuid NOT NULL,
  CONSTRAINT "WorkflowSync_pkey" PRIMARY KEY ("Name")
);

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

INSERT INTO "WorkflowSync"("Name","Lock") VALUES ('Timer', uuid_generate_v4()) 
	ON CONFLICT ("Name") DO NOTHING;

INSERT INTO "WorkflowSync"("Name","Lock") VALUES ('ServiceTimer', uuid_generate_v4()) 
	ON CONFLICT ("Name") DO NOTHING;

CREATE TABLE IF NOT EXISTS "WorkflowApprovalHistory" (
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "IdentityId" character varying(1024) NULL,
  "AllowedTo" text NULL,
  "TransitionTime" timestamp NULL,
  "Sort" bigint NULL,
	"InitialState" character varying(1024) NOT NULL,
	"DestinationState" character varying(1024) NOT NULL,
	"TriggerName" character varying(1024) NULL,
	"Commentary" text NULL,
  CONSTRAINT "WorkflowApprovalHistory_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "WorkflowApprovalHistory_ProcessId_idx"  ON "WorkflowApprovalHistory" USING btree ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowApprovalHistory_IdentityId_idx" ON "WorkflowApprovalHistory" USING btree ("IdentityId");


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

