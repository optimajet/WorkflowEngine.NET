BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWSCHEME (
  CODE NVARCHAR2(256),
  SCHEME CLOB NOT NULL,
  CANBEINLINED CHAR(1 BYTE) DEFAULT 0,
  INLINEDSCHEMES NVARCHAR2(1024),
  TAGS NVARCHAR2(1024),
  CONSTRAINT PK_WORKFLOWSCHEME PRIMARY KEY (CODE) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED ))
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
                220 AS Version,
                SYSDATE AS AppliedOn,
                'Migration220CreateTable_WorkflowScheme' AS Description
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
