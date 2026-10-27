BEGIN
EXECUTE IMMEDIATE '
CREATE TABLE WORKFLOWFORM (
                              ID RAW(16) NOT NULL,
                              NAME NVARCHAR2(512) NOT NULL,
                              VERSION NUMBER(10) NOT NULL,
                              CREATIONDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                              UPDATEDDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                              DEFINITION NCLOB NOT NULL,
                              LOCKFLAG NUMBER(10) NOT NULL,
                              CONSTRAINT PK_WORKFLOWFORM PRIMARY KEY (ID),
                              CONSTRAINT UQ_WORKFLOWFORM_NAME_VERSION UNIQUE (NAME, VERSION)
)
    LOGGING';
END;

DECLARE
    v_table_exists PLS_INTEGER;
BEGIN
    SELECT COUNT(*)
    INTO v_table_exists
    FROM ALL_TABLES
    WHERE OWNER = SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA')
      AND TABLE_NAME = 'VERSIONINFO';

    IF v_table_exists > 0 THEN
        EXECUTE IMMEDIATE q'[
        MERGE INTO VersionInfo target
        USING (
            SELECT
                400 AS Version,
                SYSDATE AS AppliedOn,
                'Migration400CreateTable_WorkflowForms' AS Description
            FROM DUAL
        ) source
        ON (target.Version = source.Version)
        WHEN NOT MATCHED THEN
            INSERT (Version, AppliedOn, Description)
            VALUES (
                       source.Version,
                       source.AppliedOn,
                       source.Description
                   )
        ]';
    END IF;
END;
/

COMMIT;
