DO $$
DECLARE
    assignment_table_has_rows boolean := false;
BEGIN
    IF to_regclass('"WorkflowProcessAssignment"') IS NOT NULL THEN
        EXECUTE 'SELECT EXISTS (SELECT 1 FROM "WorkflowProcessAssignment")'
            INTO assignment_table_has_rows;

        IF NOT assignment_table_has_rows THEN
            EXECUTE 'DROP TABLE "WorkflowProcessAssignment"';
        END IF;
    END IF;
END
$$;

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
                70,
                NOW(),
                'Migration70DropEmptyWorkflowProcessAssignmentTable'
            WHERE NOT EXISTS (
                SELECT 1
                FROM public."VersionInfo"
                WHERE "Version" = 70
            )
        $sql$;
        END IF;
    END
$$;
