BEGIN
EXECUTE IMMEDIATE 'DROP INDEX IDX_WORKFLOWPROCESSSCHEME_SCHE';
EXCEPTION
WHEN OTHERS THEN
  IF SQLCODE != -1418 THEN
    RAISE;
  END IF;
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
                690 AS Version,
                SYSDATE AS AppliedOn,
                'Migration690DropIndex_WorkflowProcessScheme_SchemeCodeHashIsObsolete' AS Description
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
