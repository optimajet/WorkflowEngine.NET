DELETE FROM "WorkflowProcessInstancePersistence"
WHERE "ProcessId" = 'F02D5A80-A2F4-464F-8098-C349CE2AEA7A'::uuid;

DELETE FROM "WorkflowProcessTransitionHistory"
WHERE "ProcessId" = 'F02D5A80-A2F4-464F-8098-C349CE2AEA7A'::uuid;

DELETE FROM "WorkflowProcessTimer"
WHERE "ProcessId" = 'F02D5A80-A2F4-464F-8098-C349CE2AEA7A'::uuid;

DELETE FROM "WorkflowInbox"
WHERE "ProcessId" = 'F02D5A80-A2F4-464F-8098-C349CE2AEA7A'::uuid;

DELETE FROM "WorkflowApprovalHistory"
WHERE "ProcessId" = 'F02D5A80-A2F4-464F-8098-C349CE2AEA7A'::uuid;

DELETE FROM "WorkflowProcessInstanceStatus"
WHERE "Id" = 'F02D5A80-A2F4-464F-8098-C349CE2AEA7A'::uuid;

DELETE FROM "WorkflowProcessInstance"
WHERE "Id" = 'F02D5A80-A2F4-464F-8098-C349CE2AEA7A'::uuid;

DELETE FROM "WorkflowProcessScheme"
WHERE "Id" = '7BF7AFA1-AFD5-482D-B090-5A9CE449EB97'::uuid
   OR "SchemeCode" = 'OptimaJet.Workflow.Plugins.AssigmentPlugin.CheckDeadlines'
   OR "RootSchemeCode" = 'OptimaJet.Workflow.Plugins.AssigmentPlugin.CheckDeadlines';

-- DbScriptPublisher: mark migration as applied
DO $$
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name = 'VersionInfo'
              AND table_type = 'BASE TABLE'
        ) THEN
            EXECUTE $sql$
            INSERT INTO public."VersionInfo"
                ("Version", "AppliedOn", "Description")
            SELECT
                60,
                NOW(),
                'Migration60RemoveAssignmentCheckDeadlinesSystemProcess'
            WHERE NOT EXISTS (
                SELECT 1
                FROM public."VersionInfo"
                WHERE "Version" = 60
            )
        $sql$;
        END IF;
    END
$$;
