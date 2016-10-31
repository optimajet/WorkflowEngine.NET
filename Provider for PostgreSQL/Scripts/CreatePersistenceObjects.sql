/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for PostgreSQL
Version: 1.5.6
File: CreatePersistenceObjects.sql
*/
-- WorkflowInbox
CREATE TABLE "WorkflowInbox"
(
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "IdentityId" character varying(256) NOT NULL,
  CONSTRAINT "WorkflowInbox_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX "WorkflowInbox_IdentityId_idx" ON "WorkflowInbox" USING btree ("IdentityId" COLLATE pg_catalog."default");
CREATE INDEX "WorkflowInbox_ProcessId_idx"  ON "WorkflowInbox" USING btree ("ProcessId");

--WorkflowProcessInstance
CREATE TABLE "WorkflowProcessInstance" (
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

CREATE INDEX "WorkflowProcessInstance_SchemeId_idx"  ON "WorkflowProcessInstance" USING btree ("SchemeId");

--WorkflowProcessInstancePersistence
CREATE TABLE IF NOT EXISTS "WorkflowProcessInstancePersistence" (
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "ParameterName" character varying(256) NOT NULL,
  "Value" text NOT NULL,
  CONSTRAINT "WorkflowProcessInstancePersistence_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX "WorkflowProcessInstancePersistence_ProcessId_idx"  ON "WorkflowProcessInstancePersistence" USING btree ("ProcessId");

--WorkflowProcessInstanceStatus
CREATE TABLE "WorkflowProcessInstanceStatus" (
  "Id" uuid NOT NULL,
  "Status" smallint NOT NULL,
  "Lock" uuid NOT NULL,
  CONSTRAINT "WorkflowProcessInstanceStatus_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX "WorkflowProcessInstanceStatus_Status_idx"  ON "WorkflowProcessInstanceStatus" USING btree ("Status");

--WorkflowProcessScheme
CREATE TABLE IF NOT EXISTS "WorkflowProcessScheme" (
  "Id" uuid NOT NULL,
  "Scheme" text NOT NULL,
  "DefiningParameters" text NOT NULL,
  "DefiningParametersHash" character varying(1024) NOT NULL,
  "SchemeCode" character varying(256) NOT NULL,
  "IsObsolete" boolean NOT NULL,
  "RootSchemeCode" character varying(256) NULL,
  "RootSchemeId" uuid NULL,
  "AllowedActivities" text NULL,
  "StartingTransition" text NULL,
  CONSTRAINT "WorkflowProcessScheme_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX "WorkflowProcessScheme_DefiningParametersHash_idx"  ON "WorkflowProcessScheme" USING btree ("DefiningParametersHash");
CREATE INDEX "WorkflowProcessScheme_SchemeCode_idx"  ON "WorkflowProcessScheme" USING btree ("SchemeCode");
CREATE INDEX "WorkflowProcessScheme_IsObsolete_idx"  ON "WorkflowProcessScheme" USING btree ("IsObsolete");

--WorkflowProcessTimer
CREATE TABLE IF NOT EXISTS "WorkflowProcessTimer" (
  "Id" uuid NOT NULL,
  "ProcessId" uuid NOT NULL,
  "Name" character varying(256) NOT NULL,
  "NextExecutionDateTime" timestamp NOT NULL,
  "Ignore" boolean NOT NULL,
  CONSTRAINT "WorkflowProcessTimer_pkey" PRIMARY KEY ("Id")
);
CREATE INDEX "WorkflowProcessTimer_ProcessId_idx"  ON "WorkflowProcessTimer" USING btree ("ProcessId");
CREATE INDEX "WorkflowProcessTimer_Name_idx"  ON "WorkflowProcessTimer" USING btree ("Name");
CREATE INDEX "WorkflowProcessTimer_NextExecutionDateTime_idx"  ON "WorkflowProcessTimer" USING btree ("NextExecutionDateTime");
CREATE INDEX "WorkflowProcessTimer_Ignore_idx"  ON "WorkflowProcessTimer" USING btree ("Ignore");

--WorkflowProcessTransitionHistory
CREATE TABLE "WorkflowProcessTransitionHistory" (
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
CREATE INDEX "WorkflowProcessTransitionHistory_ProcessId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ProcessId");
CREATE INDEX "WorkflowProcessTransitionHistory_ExecutorIdentityId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ExecutorIdentityId");
CREATE INDEX "WorkflowProcessTransitionHistory_ActorIdentityId_idx"  ON "WorkflowProcessTransitionHistory" USING btree ("ActorIdentityId");


--WorkflowScheme
CREATE TABLE "WorkflowScheme" (
  "Code" character varying(256) NOT NULL,
  "Scheme" text NOT NULL,
  CONSTRAINT "WorkflowScheme_pkey" PRIMARY KEY ("Code")
);

--WorkflowGlobalParameter
CREATE TABLE IF NOT EXISTS "WorkflowGlobalParameter" (
  "Id" uuid NOT NULL,
  "Type" character varying(256) NOT NULL,
  "Name" character varying(256) NOT NULL,
  "Value" text NOT NULL,
   CONSTRAINT "WorkflowGlobalParameter_pkey" PRIMARY KEY ("Id")
);
