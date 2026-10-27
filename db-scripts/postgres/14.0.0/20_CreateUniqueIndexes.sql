CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowInbox_ProcessId_IdentityId_idx" ON "WorkflowInbox" USING btree ("ProcessId", "IdentityId");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowProcessInstancePersistence_ProcessId_ParameterName_idx" ON "WorkflowProcessInstancePersistence" USING btree ("ProcessId", "ParameterName");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowProcessTimer_ProcessId_Name_idx" ON "WorkflowProcessTimer" USING btree ("ProcessId", "Name");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Type_Name_idx" ON "WorkflowGlobalParameter" USING btree ("Type", "Name");

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
                20,
                NOW(),
                'Migration20CreateUniqueIndexes'
            WHERE NOT EXISTS (
                SELECT 1
                FROM public."VersionInfo"
                WHERE "Version" = 20
            )
        $sql$;
        END IF;
    END
$$;
