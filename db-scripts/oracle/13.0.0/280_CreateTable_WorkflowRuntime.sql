BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWRUNTIME (
  RUNTIMEID NVARCHAR2(900),
  STATUS NUMBER(3) NOT NULL,
  RESTORERID NVARCHAR2(900),
  LOCKFLAG RAW(16) NOT NULL,
  NEXTTIMERTIME TIMESTAMP NULL,
  NEXTSERVICETIMERTIME TIMESTAMP NULL,
  LASTALIVESIGNAL TIMESTAMP NULL,
  CONSTRAINT PK_WORKFLOWRUNTIME PRIMARY KEY (RUNTIMEID) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED ))
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
                280 AS Version,
                SYSDATE AS AppliedOn,
                'Migration280CreateTable_WorkflowRuntime' AS Description
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
