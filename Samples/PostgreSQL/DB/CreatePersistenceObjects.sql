/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 3.3
File: CreatePersistenceObjects.sql
*/
-- WorkflowInbox
CREATE TABLE IF NOT EXISTS "WorkflowInbox"
(
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "IdentityId" character varying(256) NOT NULL,
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
  CONSTRAINT "WorkflowProcessInstance_pkey" PRIMARY KEY ("Id")
);

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
  CONSTRAINT "WorkflowProcessInstanceStatus_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "WorkflowProcessInstanceStatus_Status_idx"  ON "WorkflowProcessInstanceStatus" USING btree ("Status");

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
  "Name" character varying(256) NOT NULL,
  "NextExecutionDateTime" timestamp NOT NULL,
  "Ignore" boolean NOT NULL,
  CONSTRAINT "WorkflowProcessTimer_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_ProcessId_idx"  ON "WorkflowProcessTimer" USING btree ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_Name_idx"  ON "WorkflowProcessTimer" USING btree ("Name");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_NextExecutionDateTime_idx"  ON "WorkflowProcessTimer" USING btree ("NextExecutionDateTime");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_Ignore_idx"  ON "WorkflowProcessTimer" USING btree ("Ignore");

--WorkflowProcessTransitionHistory
CREATE TABLE IF NOT EXISTS "WorkflowProcessTransitionHistory" (
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "ExecutorIdentityId" character varying(256) NULL,
  "ActorIdentityId" character varying(256) NULL,
  "FromActivityName" character varying(256) NOT NULL,
  "ToActivityName" character varying(256) NOT NULL,
  "ToStateName" character varying(256) NULL,
  "TransitionTime" timestamp NOT NULL,
  "TransitionClassifier" character varying(256) NOT NULL,
  "FromStateName" character varying(256) NULL,
  "TriggerName" character varying(256) NULL,
  "IsFinalised" boolean NOT NULL,
  CONSTRAINT "WorkflowProcessTransitionHistory_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ProcessId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ExecutorIdentityId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ExecutorIdentityId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ActorIdentityId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ActorIdentityId");


--WorkflowScheme
CREATE TABLE IF NOT EXISTS "WorkflowScheme" (
  "Code" character varying(256) NOT NULL,
  "Scheme" text NOT NULL,
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
