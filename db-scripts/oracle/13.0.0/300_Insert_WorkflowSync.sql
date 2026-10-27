BEGIN
EXECUTE IMMEDIATE
    'INSERT INTO WORKFLOWSYNC (Name, LOCKFLAG) 
    SELECT  ''Timer'', sys_guid() 
    FROM DUAL 
    WHERE NOT EXISTS (
        SELECT 1 FROM WORKFLOWSYNC WHERE Name = ''Timer''
    )';
EXECUTE IMMEDIATE
    'INSERT INTO WORKFLOWSYNC (Name, LOCKFLAG) 
    SELECT  ''ServiceTimer'', sys_guid() 
    FROM DUAL WHERE NOT EXISTS (
        SELECT 1 FROM WORKFLOWSYNC WHERE Name = ''ServiceTimer''
    )';
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
                300 AS Version,
                SYSDATE AS AppliedOn,
                'Migration300Insert_WorkflowSync' AS Description
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
