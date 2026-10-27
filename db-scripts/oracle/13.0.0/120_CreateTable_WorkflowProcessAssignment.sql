BEGIN
    EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWPROCESSASSIGNMENT (
        ID RAW(16),
        ASSIGNMENTCODE VARCHAR2(256 BYTE) NOT NULL,
        PROCESSID RAW(16) NOT NULL,
        NAME VARCHAR2(256 BYTE) NOT NULL,
        DESCRIPTION VARCHAR2(256 BYTE),
        STATUSSTATE VARCHAR2(256 BYTE) NOT NULL,
        DATECREATION TIMESTAMP NOT NULL,
        DATESTART TIMESTAMP,
        DATEFINISH TIMESTAMP,
        DEADLINETOSTART TIMESTAMP,
        DEADLINETOCOMPLETE TIMESTAMP,
        EXECUTOR VARCHAR2(256 BYTE) NOT NULL,
        OBSERVERS VARCHAR2(256 BYTE),
        TAGS VARCHAR2(256 BYTE),
        ISACTIVE CHAR(1 BYTE) DEFAULT 0 NOT NULL,
        ISDELETED CHAR(1 BYTE) DEFAULT 0 NOT NULL,
        CONSTRAINT PK_WORKFLOWPROCESSASSIGNMENT PRIMARY KEY (ID) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED ))
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
                120 AS Version,
                SYSDATE AS AppliedOn,
                'Migration120CreateTable_WorkflowProcessAssignment' AS Description
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
