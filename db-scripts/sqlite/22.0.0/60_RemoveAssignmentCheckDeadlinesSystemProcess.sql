DELETE FROM "WorkflowProcessInstancePersistence"
WHERE "ProcessId" = 'f02d5a80-a2f4-464f-8098-c349ce2aea7a';

DELETE FROM "WorkflowProcessTransitionHistory"
WHERE "ProcessId" = 'f02d5a80-a2f4-464f-8098-c349ce2aea7a';

DELETE FROM "WorkflowProcessTimer"
WHERE "ProcessId" = 'f02d5a80-a2f4-464f-8098-c349ce2aea7a';

DELETE FROM "WorkflowInbox"
WHERE "ProcessId" = 'f02d5a80-a2f4-464f-8098-c349ce2aea7a';

DELETE FROM "WorkflowApprovalHistory"
WHERE "ProcessId" = 'f02d5a80-a2f4-464f-8098-c349ce2aea7a';

DELETE FROM "WorkflowProcessInstanceStatus"
WHERE "Id" = 'f02d5a80-a2f4-464f-8098-c349ce2aea7a';

DELETE FROM "WorkflowProcessInstance"
WHERE "Id" = 'f02d5a80-a2f4-464f-8098-c349ce2aea7a';

DELETE FROM "WorkflowProcessScheme"
WHERE "Id" = '7bf7afa1-afd5-482d-b090-5a9ce449eb97'
   OR "SchemeCode" = 'OptimaJet.Workflow.Plugins.AssigmentPlugin.CheckDeadlines'
   OR "RootSchemeCode" = 'OptimaJet.Workflow.Plugins.AssigmentPlugin.CheckDeadlines';

-- DbScriptPublisher: mark migration as applied
-- !!! Remove this block if your database does not contain the "VersionInfo" table created by FluentMigrator. !!!
INSERT INTO "VersionInfo" ("Version", "AppliedOn", "Description")
SELECT 60, CURRENT_TIMESTAMP, 'Migration60RemoveAssignmentCheckDeadlinesSystemProcess'
WHERE NOT EXISTS (
    SELECT 1
    FROM "VersionInfo"
    WHERE "Version" = 60
);
