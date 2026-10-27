ALTER TABLE "WorkflowProcessInstance"
    ADD COLUMN IF NOT EXISTS "CalendarName" character varying(256) null;

CREATE INDEX IF NOT EXISTS "IX_CalendarName" ON "WorkflowProcessInstance" USING btree ("CalendarName");

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
                15,
                NOW(),
                'Migration15CreateCalendarNameColumn'
            WHERE NOT EXISTS (
                SELECT 1
                FROM public."VersionInfo"
                WHERE "Version" = 15
            )
        $sql$;
        END IF;
    END
$$;
