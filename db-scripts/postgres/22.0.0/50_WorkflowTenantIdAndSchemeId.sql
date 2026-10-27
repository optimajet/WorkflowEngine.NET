DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM "WorkflowGlobalParameter"
        WHERE length("Type") > 128
    ) THEN
        RAISE EXCEPTION 'BREAKING CHANGES DETECTED: Some rows in the Type column in WorkflowGlobalParameter table are longer than 128 characters. Please contact support support@optimajet.com.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM "WorkflowProcessInstance"
        WHERE length("TenantId") > 128
    ) THEN
        RAISE EXCEPTION 'BREAKING CHANGES DETECTED: Some rows in the TenantId column in WorkflowProcessInstance table are longer than 128 characters. Please contact support support@optimajet.com.';
    END IF;
END
$$;

DROP INDEX IF EXISTS "WorkflowGlobalParameter_Type_Name_idx";

ALTER TABLE "WorkflowGlobalParameter"
    ALTER COLUMN "Type" TYPE character varying(128);

ALTER TABLE "WorkflowGlobalParameter"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Type_Name_TenantId_idx"
    ON "WorkflowGlobalParameter" USING btree ("Type", "Name", "TenantId") NULLS NOT DISTINCT;

ALTER TABLE "WorkflowScheme"
    ADD COLUMN IF NOT EXISTS "Id" uuid NULL;

UPDATE "WorkflowScheme"
SET "Id" = uuid_generate_v4()
WHERE "Id" IS NULL;

ALTER TABLE "WorkflowScheme"
    ALTER COLUMN "Id" SET NOT NULL;

ALTER TABLE "WorkflowScheme"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

ALTER TABLE "WorkflowScheme"
    DROP CONSTRAINT IF EXISTS "WorkflowScheme_pkey";

ALTER TABLE "WorkflowScheme"
    ADD CONSTRAINT "WorkflowScheme_pkey" PRIMARY KEY ("Id");

DROP INDEX IF EXISTS "WorkflowScheme_Code_TenantId_idx";

CREATE UNIQUE INDEX "WorkflowScheme_Code_TenantId_idx"
    ON "WorkflowScheme" USING btree ("Code", "TenantId") NULLS NOT DISTINCT;

ALTER TABLE "WorkflowForm"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

ALTER TABLE "WorkflowForm"
    DROP CONSTRAINT IF EXISTS "WorkflowForm_Name_Version_key";

DROP INDEX IF EXISTS "WorkflowForm_Name_Version_key";

CREATE UNIQUE INDEX "WorkflowForm_Name_Version_TenantId_key"
    ON "WorkflowForm" USING btree ("Name", "Version", "TenantId") NULLS NOT DISTINCT;

ALTER TABLE "WorkflowProcessScheme"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

DROP INDEX IF EXISTS "WorkflowProcessScheme_SchemeCode_idx";
DROP INDEX IF EXISTS "WorkflowProcessScheme_IsObsolete_idx";

CREATE INDEX IF NOT EXISTS "WorkflowProcessScheme_SchemeCode_TenantId_RootSchemeId_Obs_idx"
    ON "WorkflowProcessScheme" USING btree ("SchemeCode", "TenantId", "RootSchemeId", "IsObsolete");

ALTER TABLE "WorkflowApprovalHistory"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

ALTER TABLE "WorkflowInbox"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

ALTER TABLE "WorkflowProcessInstance"
    ALTER COLUMN "TenantId" TYPE character varying(128);

ALTER TABLE "WorkflowProcessInstancePersistence"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

ALTER TABLE "WorkflowProcessInstanceStatus"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

ALTER TABLE "WorkflowProcessTimer"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

ALTER TABLE "WorkflowProcessTransitionHistory"
    ADD COLUMN IF NOT EXISTS "TenantId" character varying(128) NULL;

UPDATE "WorkflowProcessInstanceStatus" status_table
SET "TenantId" = process_table."TenantId"
FROM "WorkflowProcessInstance" process_table
WHERE process_table."Id" = status_table."Id"
  AND process_table."TenantId" IS NOT NULL
  AND status_table."TenantId" IS DISTINCT FROM process_table."TenantId";

UPDATE "WorkflowApprovalHistory" history_table
SET "TenantId" = process_table."TenantId"
FROM "WorkflowProcessInstance" process_table
WHERE process_table."Id" = history_table."ProcessId"
  AND process_table."TenantId" IS NOT NULL
  AND history_table."TenantId" IS DISTINCT FROM process_table."TenantId";

UPDATE "WorkflowInbox" inbox_table
SET "TenantId" = process_table."TenantId"
FROM "WorkflowProcessInstance" process_table
WHERE process_table."Id" = inbox_table."ProcessId"
  AND process_table."TenantId" IS NOT NULL
  AND inbox_table."TenantId" IS DISTINCT FROM process_table."TenantId";

UPDATE "WorkflowProcessInstancePersistence" persistence_table
SET "TenantId" = process_table."TenantId"
FROM "WorkflowProcessInstance" process_table
WHERE process_table."Id" = persistence_table."ProcessId"
  AND process_table."TenantId" IS NOT NULL
  AND persistence_table."TenantId" IS DISTINCT FROM process_table."TenantId";

UPDATE "WorkflowProcessTimer" timer_table
SET "TenantId" = process_table."TenantId"
FROM "WorkflowProcessInstance" process_table
WHERE process_table."Id" = timer_table."ProcessId"
  AND process_table."TenantId" IS NOT NULL
  AND timer_table."TenantId" IS DISTINCT FROM process_table."TenantId";

UPDATE "WorkflowProcessTransitionHistory" transition_table
SET "TenantId" = process_table."TenantId"
FROM "WorkflowProcessInstance" process_table
WHERE process_table."Id" = transition_table."ProcessId"
  AND process_table."TenantId" IS NOT NULL
  AND transition_table."TenantId" IS DISTINCT FROM process_table."TenantId";

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
                50,
                NOW(),
                'Migration50WorkflowTenantIdAndSchemeId'
            WHERE NOT EXISTS (
                SELECT 1
                FROM public."VersionInfo"
                WHERE "Version" = 50
            )
        $sql$;
        END IF;
    END
$$;
