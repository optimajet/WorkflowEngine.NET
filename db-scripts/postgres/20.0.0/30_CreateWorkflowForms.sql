CREATE TABLE IF NOT EXISTS "WorkflowForm" (
    "Id" uuid NOT NULL,
    "Name"  character varying(512) NOT NULL,
    "Version" integer NOT NULL,
    "CreationDate" timestamp NOT NULL DEFAULT localtimestamp,
    "UpdatedDate" timestamp NOT NULL DEFAULT localtimestamp,
    "Definition" text NOT NULL,
    "Lock" integer NOT NULL,
    CONSTRAINT "WorkflowForm_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "WorkflowForm_Name_Version_key" UNIQUE ("Name", "Version")
    );

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
                30,
                NOW(),
                'Migration30CreateWorkflowForms'
            WHERE NOT EXISTS (
                SELECT 1
                FROM public."VersionInfo"
                WHERE "Version" = 30
            )
        $sql$;
        END IF;
    END
$$;
