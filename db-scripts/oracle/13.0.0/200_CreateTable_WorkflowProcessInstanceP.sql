BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWPROCESSINSTANCEP (
  ID RAW(16),
  PROCESSID RAW(16) NOT NULL,
  PARAMETERNAME NVARCHAR2(256) NOT NULL,
  VALUE NCLOB NOT NULL,
  CONSTRAINT PK_WORKFLOWPROCESSINSTANCEP PRIMARY KEY (ID) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED ))
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
                200 AS Version,
                SYSDATE AS AppliedOn,
                'Migration200CreateTable_WorkflowProcessInstanceP' AS Description
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
