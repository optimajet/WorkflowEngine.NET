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