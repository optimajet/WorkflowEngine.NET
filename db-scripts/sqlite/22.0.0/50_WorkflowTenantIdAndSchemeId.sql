CREATE TEMP TABLE "__precheck_workflowglobalparameter_type" (
    "IsValid" INTEGER NOT NULL CHECK ("IsValid" = 1)
);

INSERT INTO "__precheck_workflowglobalparameter_type" ("IsValid")
VALUES (
    CASE
        WHEN EXISTS (
            SELECT 1
            FROM "WorkflowGlobalParameter"
            WHERE length("Type") > 128
        ) THEN 0
        ELSE 1
    END
);

DROP TABLE "__precheck_workflowglobalparameter_type";

CREATE TEMP TABLE "__precheck_workflowprocessinstance_tenantid" (
    "IsValid" INTEGER NOT NULL CHECK ("IsValid" = 1)
);

INSERT INTO "__precheck_workflowprocessinstance_tenantid" ("IsValid")
VALUES (
    CASE
        WHEN EXISTS (
            SELECT 1
            FROM "WorkflowProcessInstance"
            WHERE length("TenantId") > 128
        ) THEN 0
        ELSE 1
    END
);

DROP TABLE "__precheck_workflowprocessinstance_tenantid";

ALTER TABLE "WorkflowGlobalParameter"
    ADD COLUMN "TenantId" TEXT NULL;

DROP INDEX IF EXISTS "WorkflowGlobalParameter_Type_Name_idx";
DROP INDEX IF EXISTS "WorkflowGlobalParameter_Type_Name_TenantId_idx";

CREATE UNIQUE INDEX "WorkflowGlobalParameter_Type_Name_TenantId_idx"
    ON "WorkflowGlobalParameter" ("Type", "Name", ifnull("TenantId", char(0)));

CREATE TABLE "WorkflowScheme_tmp"
(
    "Id"             TEXT    NOT NULL,
    "Code"           TEXT    NOT NULL,
    "Scheme"         TEXT    NOT NULL,
    "CanBeInlined"   INTEGER NOT NULL DEFAULT 0,
    "InlinedSchemes" TEXT    NULL,
    "Tags"           TEXT    NULL,
    "TenantId"       TEXT    NULL,
    CONSTRAINT "WorkflowScheme_pkey" PRIMARY KEY ("Id")
);

INSERT INTO "WorkflowScheme_tmp" ("Id", "Code", "Scheme", "CanBeInlined", "InlinedSchemes", "Tags", "TenantId")
SELECT lower(hex(randomblob(16))), "Code", "Scheme", "CanBeInlined", "InlinedSchemes", "Tags", NULL
FROM "WorkflowScheme";

DROP TABLE "WorkflowScheme";

ALTER TABLE "WorkflowScheme_tmp" RENAME TO "WorkflowScheme";

CREATE UNIQUE INDEX "WorkflowScheme_Code_TenantId_idx"
    ON "WorkflowScheme" ("Code", ifnull("TenantId", char(0)));

CREATE TABLE "WorkflowForm_tmp"
(
    "Id"           TEXT    NOT NULL,
    "Name"         TEXT    NOT NULL,
    "Version"      INTEGER NOT NULL,
    "CreationDate" INTEGER NOT NULL,
    "UpdatedDate"  INTEGER NOT NULL,
    "Definition"   TEXT    NOT NULL,
    "Lock"         INTEGER NOT NULL,
    "TenantId"     TEXT    NULL,
    CONSTRAINT "WorkflowForm_pkey" PRIMARY KEY ("Id")
);

INSERT INTO "WorkflowForm_tmp" (
    "Id",
    "Name",
    "Version",
    "CreationDate",
    "UpdatedDate",
    "Definition",
    "Lock",
    "TenantId"
)
SELECT
    "Id",
    "Name",
    "Version",
    "CreationDate",
    "UpdatedDate",
    "Definition",
    "Lock",
    NULL
FROM "WorkflowForm";

DROP TABLE "WorkflowForm";

ALTER TABLE "WorkflowForm_tmp" RENAME TO "WorkflowForm";

CREATE UNIQUE INDEX "WorkflowForm_Name_Version_TenantId_idx"
    ON "WorkflowForm" ("Name", "Version", ifnull("TenantId", char(0)));

ALTER TABLE "WorkflowProcessScheme"
    ADD COLUMN "TenantId" TEXT NULL;

DROP INDEX IF EXISTS "WorkflowProcessScheme_SchemeCode_idx";
DROP INDEX IF EXISTS "WorkflowProcessScheme_IsObsolete_idx";

CREATE INDEX IF NOT EXISTS "WorkflowProcessScheme_SchemeCode_TenantId_RootSchemeId_Obs_idx"
    ON "WorkflowProcessScheme" ("SchemeCode", "TenantId", "RootSchemeId", "IsObsolete");

ALTER TABLE "WorkflowApprovalHistory"
    ADD COLUMN "TenantId" TEXT NULL;

ALTER TABLE "WorkflowInbox"
    ADD COLUMN "TenantId" TEXT NULL;

ALTER TABLE "WorkflowProcessInstancePersistence"
    ADD COLUMN "TenantId" TEXT NULL;

ALTER TABLE "WorkflowProcessInstanceStatus"
    ADD COLUMN "TenantId" TEXT NULL;

ALTER TABLE "WorkflowProcessTimer"
    ADD COLUMN "TenantId" TEXT NULL;

ALTER TABLE "WorkflowProcessTransitionHistory"
    ADD COLUMN "TenantId" TEXT NULL;

UPDATE "WorkflowProcessInstanceStatus" AS target
SET "TenantId" = source."TenantId"
FROM "WorkflowProcessInstance" AS source
WHERE source."Id" = target."Id"
  AND source."TenantId" IS NOT NULL
  AND (target."TenantId" IS NULL OR target."TenantId" <> source."TenantId");

UPDATE "WorkflowApprovalHistory" AS target
SET "TenantId" = source."TenantId"
FROM "WorkflowProcessInstance" AS source
WHERE source."Id" = target."ProcessId"
  AND source."TenantId" IS NOT NULL
  AND (target."TenantId" IS NULL OR target."TenantId" <> source."TenantId");

UPDATE "WorkflowInbox" AS target
SET "TenantId" = source."TenantId"
FROM "WorkflowProcessInstance" AS source
WHERE source."Id" = target."ProcessId"
  AND source."TenantId" IS NOT NULL
  AND (target."TenantId" IS NULL OR target."TenantId" <> source."TenantId");

UPDATE "WorkflowProcessInstancePersistence" AS target
SET "TenantId" = source."TenantId"
FROM "WorkflowProcessInstance" AS source
WHERE source."Id" = target."ProcessId"
  AND source."TenantId" IS NOT NULL
  AND (target."TenantId" IS NULL OR target."TenantId" <> source."TenantId");

UPDATE "WorkflowProcessTimer" AS target
SET "TenantId" = source."TenantId"
FROM "WorkflowProcessInstance" AS source
WHERE source."Id" = target."ProcessId"
  AND source."TenantId" IS NOT NULL
  AND (target."TenantId" IS NULL OR target."TenantId" <> source."TenantId");

UPDATE "WorkflowProcessTransitionHistory" AS target
SET "TenantId" = source."TenantId"
FROM "WorkflowProcessInstance" AS source
WHERE source."Id" = target."ProcessId"
  AND source."TenantId" IS NOT NULL
  AND (target."TenantId" IS NULL OR target."TenantId" <> source."TenantId");

-- DbScriptPublisher: mark migration as applied
-- !!! Remove this block if your database does not contain the "VersionInfo" table created by FluentMigrator. !!!
INSERT INTO "VersionInfo" ("Version", "AppliedOn", "Description")
SELECT 50, CURRENT_TIMESTAMP, 'Migration50WorkflowTenantIdAndSchemeId'
WHERE NOT EXISTS (
    SELECT 1
    FROM "VersionInfo"
    WHERE "Version" = 50
);
