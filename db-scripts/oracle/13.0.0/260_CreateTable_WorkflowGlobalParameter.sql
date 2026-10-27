BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWGLOBALPARAMETER (
  ID RAW(16),
  TYPE NVARCHAR2(512)  NOT NULL,
  NAME NVARCHAR2(256)  NOT NULL,
  VALUE CLOB NOT NULL,
  CONSTRAINT PK_WORKFLOWGLOBALPARAMETER PRIMARY KEY (ID) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED ))
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
                260 AS Version,
                SYSDATE AS AppliedOn,
                'Migration260CreateTable_WorkflowGlobalParameter' AS Description
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
