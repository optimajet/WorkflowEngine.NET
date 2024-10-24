/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for SQLite
Version: 16.0
File: CreatePersistenceObjects.sql
*/

CREATE TABLE IF NOT EXISTS "WorkflowInbox"
(
    "Id"                TEXT    NOT NULL,
    "ProcessId"         TEXT    NOT NULL,
    "IdentityId"        TEXT    NOT NULL,
    "AddingDate"        INTEGER NOT NULL DEFAULT ((CAST(strftime('%s', 'now') AS INTEGER) * 10000000) + 621355968000000000),
    "AvailableCommands" TEXT    NOT NULL DEFAULT '',
    CONSTRAINT "WorkflowInbox_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowInbox_IdentityId_idx" ON "WorkflowInbox" ("IdentityId");
CREATE INDEX IF NOT EXISTS "WorkflowInbox_ProcessId_idx" ON "WorkflowInbox" ("ProcessId");

CREATE TABLE IF NOT EXISTS "WorkflowProcessInstance"
(
    "Id"                             TEXT    NOT NULL,
    "StateName"                      TEXT    NULL,
    "ActivityName"                   TEXT    NOT NULL,
    "SchemeId"                       TEXT    NOT NULL,
    "PreviousState"                  TEXT    NULL,
    "PreviousStateForDirect"         TEXT    NULL,
    "PreviousStateForReverse"        TEXT    NULL,
    "PreviousActivity"               TEXT    NULL,
    "PreviousActivityForDirect"      TEXT    NULL,
    "PreviousActivityForReverse"     TEXT    NULL,
    "IsDeterminingParametersChanged" INTEGER NOT NULL,
    "ParentProcessId"                TEXT    NULL,
    "RootProcessId"                  TEXT    NOT NULL,
    "TenantId"                       TEXT    NULL,
    "StartingTransition"             TEXT    NULL,
    "SubprocessName"                 TEXT    NULL,
    "CreationDate"                   INTEGER NOT NULL DEFAULT ((CAST(strftime('%s', 'now') AS INTEGER) * 10000000) + 621355968000000000),
    "LastTransitionDate"             INTEGER NULL,
    "CalendarName"                   TEXT    NULL,
    CONSTRAINT "WorkflowProcessInstance_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessInstance_RootProcessId_idx" ON "WorkflowProcessInstance" ("RootProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessInstance_CalendarName_idx" ON "WorkflowProcessInstance" ("CalendarName");

CREATE TABLE IF NOT EXISTS "WorkflowProcessInstancePersistence"
(
    "Id"            TEXT NOT NULL,
    "ProcessId"     TEXT NOT NULL,
    "ParameterName" TEXT NOT NULL,
    "Value"         TEXT NOT NULL,
    CONSTRAINT "WorkflowProcessInstancePersistence_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessInstancePersistence_ProcessId_idx" ON "WorkflowProcessInstancePersistence" ("ProcessId");

CREATE TABLE IF NOT EXISTS "WorkflowProcessInstanceStatus"
(
    "Id"        TEXT    NOT NULL,
    "Status"    INTEGER NOT NULL,
    "Lock"      TEXT    NOT NULL,
    "RuntimeId" TEXT    NOT NULL,
    "SetTime"   INTEGER NOT NULL,
    CONSTRAINT "WorkflowProcessInstanceStatus_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessInstanceStatus_Status_idx" ON "WorkflowProcessInstanceStatus" ("Status");
CREATE INDEX IF NOT EXISTS "WorkflowProcessInstanceStatus_Status_Runtime_idx" ON "WorkflowProcessInstanceStatus" ("Status" ASC, "RuntimeId" ASC);

CREATE TABLE IF NOT EXISTS "WorkflowProcessScheme"
(
    "Id"                     TEXT    NOT NULL,
    "Scheme"                 TEXT    NOT NULL,
    "DefiningParameters"     TEXT    NOT NULL,
    "DefiningParametersHash" TEXT    NOT NULL,
    "SchemeCode"             TEXT    NOT NULL,
    "IsObsolete"             INTEGER NOT NULL,
    "RootSchemeCode"         TEXT    NULL,
    "RootSchemeId"           TEXT    NULL,
    "AllowedActivities"      TEXT    NULL,
    "StartingTransition"     TEXT    NULL,
    CONSTRAINT "WorkflowProcessScheme_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessScheme_DefiningParametersHash_idx" ON "WorkflowProcessScheme" ("DefiningParametersHash");
CREATE INDEX IF NOT EXISTS "WorkflowProcessScheme_SchemeCode_idx" ON "WorkflowProcessScheme" ("SchemeCode");
CREATE INDEX IF NOT EXISTS "WorkflowProcessScheme_IsObsolete_idx" ON "WorkflowProcessScheme" ("IsObsolete");

CREATE TABLE IF NOT EXISTS "WorkflowProcessTimer"
(
    "Id"                    TEXT    NOT NULL,
    "ProcessId"             TEXT    NOT NULL,
    "RootProcessId"         TEXT    NOT NULL,
    "Name"                  TEXT    NOT NULL,
    "NextExecutionDateTime" INTEGER NOT NULL,
    "Ignore"                INTEGER NOT NULL,
    CONSTRAINT "WorkflowProcessTimer_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_ProcessId_idx" ON "WorkflowProcessTimer" ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_Name_idx" ON "WorkflowProcessTimer" ("Name");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_NextExecutionDateTime_idx" ON "WorkflowProcessTimer" ("NextExecutionDateTime");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_Ignore_idx" ON "WorkflowProcessTimer" ("Ignore");

CREATE TABLE IF NOT EXISTS "WorkflowProcessAssignment"
(
    "Id"                 TEXT    NOT NULL,
    "AssignmentCode"     TEXT    NOT NULL,
    "ProcessId"          TEXT    NOT NULL,
    "Name"               TEXT    NOT NULL,
    "Description"        TEXT    NULL,
    "StatusState"        TEXT    NOT NULL,
    "DateCreation"       INTEGER NOT NULL,
    "DateStart"          INTEGER NULL,
    "DateFinish"         INTEGER NULL,
    "DeadlineToStart"    INTEGER NULL,
    "DeadlineToComplete" INTEGER NULL,
    "Executor"           TEXT    NOT NULL,
    "Observers"          TEXT    NULL,
    "Tags"               TEXT    NULL,
    "IsActive"           INTEGER NOT NULL,
    "IsDeleted"          INTEGER NOT NULL,
    CONSTRAINT "WorkflowProcessAssignment_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessAssignment_ProcessId_idx" ON "WorkflowProcessAssignment" ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_AssignmentCode_idx" ON "WorkflowProcessAssignment" ("AssignmentCode");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_Executor_idx" ON "WorkflowProcessAssignment" ("Executor");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTimer_ProcessId_Executor_idx" ON "WorkflowProcessAssignment" ("ProcessId", "Executor");

CREATE TABLE IF NOT EXISTS "WorkflowProcessTransitionHistory"
(
    "Id"                   TEXT    NOT NULL,
    "ProcessId"            TEXT    NOT NULL,
    "ExecutorIdentityId"   TEXT    NULL,
    "ActorIdentityId"      TEXT    NULL,
    "ExecutorName"         TEXT    NULL,
    "ActorName"            TEXT    NULL,
    "FromActivityName"     TEXT    NOT NULL,
    "ToActivityName"       TEXT    NOT NULL,
    "ToStateName"          TEXT    NULL,
    "TransitionTime"       INTEGER NOT NULL,
    "TransitionClassifier" TEXT    NOT NULL,
    "FromStateName"        TEXT    NULL,
    "TriggerName"          TEXT    NULL,
    "IsFinalised"          INTEGER NOT NULL,
    "StartTransitionTime"  INTEGER NULL,
    "TransitionDuration"   INTEGER NULL,
    CONSTRAINT "WorkflowProcessTransitionHistory_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ProcessId_idx" ON "WorkflowProcessTransitionHistory" ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ExecutorIdentityId_idx" ON "WorkflowProcessTransitionHistory" ("ExecutorIdentityId");
CREATE INDEX IF NOT EXISTS "WorkflowProcessTransitionHistory_ActorIdentityId_idx" ON "WorkflowProcessTransitionHistory" ("ActorIdentityId");

CREATE TABLE IF NOT EXISTS "WorkflowScheme"
(
    "Code"           TEXT    NOT NULL,
    "Scheme"         TEXT    NOT NULL,
    "CanBeInlined"   INTEGER NOT NULL DEFAULT FALSE,
    "InlinedSchemes" TEXT    NULL,
    "Tags"           TEXT    NULL,
    CONSTRAINT "WorkflowScheme_pkey" PRIMARY KEY ("Code")
);

CREATE TABLE IF NOT EXISTS "WorkflowGlobalParameter"
(
    "Id"    TEXT NOT NULL,
    "Type"  TEXT NOT NULL,
    "Name"  TEXT NOT NULL,
    "Value" TEXT NOT NULL,
    CONSTRAINT "WorkflowGlobalParameter_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Type_idx" ON "WorkflowGlobalParameter" ("Type");
CREATE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Name_idx" ON "WorkflowGlobalParameter" ("Name");

CREATE TABLE IF NOT EXISTS "WorkflowRuntime"
(
    "RuntimeId"            TEXT    NOT NULL,
    "Lock"                 TEXT    NOT NULL,
    "Status"               INTEGER NOT NULL,
    "RestorerId"           TEXT    NULL,
    "NextTimerTime"        INTEGER NULL,
    "NextServiceTimerTime" INTEGER NULL,
    "LastAliveSignal"      INTEGER NULL,
    CONSTRAINT "WorkflowRuntime_pkey" PRIMARY KEY ("RuntimeId")
);

CREATE TABLE IF NOT EXISTS "WorkflowSync"
(
    "Name" TEXT NOT NULL,
    "Lock" TEXT NOT NULL,
    CONSTRAINT "WorkflowSync_pkey" PRIMARY KEY ("Name")
);

INSERT INTO WorkflowSync(Name, Lock)
VALUES ('Timer', 'b5874474-7d95-4731-a310-805c8981fca2')
ON CONFLICT (Name) DO NOTHING;

INSERT INTO WorkflowSync(Name, Lock)
VALUES ('ServiceTimer', 'cef386c5-0dca-2e35-d076-412d648e90c2')
ON CONFLICT (Name) DO NOTHING;

CREATE TABLE IF NOT EXISTS "WorkflowApprovalHistory"
(
    "Id"               TEXT    NOT NULL,
    "ProcessId"        TEXT    NOT NULL,
    "IdentityId"       TEXT    NULL,
    "AllowedTo"        TEXT    NULL,
    "TransitionTime"   INTEGER NULL,
    "Sort"             INTEGER NULL,
    "InitialState"     TEXT    NOT NULL,
    "DestinationState" TEXT    NOT NULL,
    "TriggerName"      TEXT    NULL,
    "Commentary"       TEXT    NULL,
    CONSTRAINT "WorkflowApprovalHistory_pkey" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "WorkflowApprovalHistory_ProcessId_idx" ON "WorkflowApprovalHistory" ("ProcessId");
CREATE INDEX IF NOT EXISTS "WorkflowApprovalHistory_IdentityId_idx" ON "WorkflowApprovalHistory" ("IdentityId");
