BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWPROCESSTIMER (
  ID RAW(16),
  PROCESSID RAW(16) NOT NULL,
  ROOTPROCESSID RAW(16) NOT NULL,
  NAME VARCHAR2(256 BYTE) NOT NULL,
  NEXTEXECUTIONDATETIME TIMESTAMP NOT NULL,
  IGNORE CHAR(1 BYTE) NOT NULL,
  CONSTRAINT PK_WORKFLOWPROCESSTIMER PRIMARY KEY (ID) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED ))
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
                90 AS Version,
                SYSDATE AS AppliedOn,
                'Migration90CreateTable_WorkflowProcessTimer' AS Description
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
