CREATE TABLE IF NOT EXISTS WorkflowForm (
                                            Id TEXT NOT NULL,
                                            Name TEXT NOT NULL,
                                            Version INTEGER NOT NULL,
                                            CreationDate INTEGER NOT NULL DEFAULT ((CAST(strftime('%s', 'now') AS INTEGER) * 10000000) + 621355968000000000),
                                            UpdatedDate INTEGER NOT NULL DEFAULT ((CAST(strftime('%s', 'now') AS INTEGER) * 10000000) + 621355968000000000),
                                            Definition TEXT NOT NULL,
                                            Lock INTEGER NOT NULL,
                                            PRIMARY KEY (Id),
    UNIQUE (Name, Version)
    );

-- DbScriptPublisher: mark migration as applied
-- !!! Remove this block if your database does not contain the "VersionInfo" table created by FluentMigrator. !!!
INSERT INTO "VersionInfo" ("Version", "AppliedOn", "Description")
SELECT 30, CURRENT_TIMESTAMP, 'Migration30CreateWorkflowForms'
WHERE NOT EXISTS (
    SELECT 1
    FROM "VersionInfo"
    WHERE "Version" = 30
);
