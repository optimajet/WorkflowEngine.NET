CREATE OR REPLACE FUNCTION DropUnusedWorkflowProcessScheme
    RETURN NUMBER
    IS
    st NUMBER := 0;
BEGIN
    DELETE FROM WorkflowProcessScheme
    WHERE WorkflowProcessScheme.IsObsolete = 1
      AND NOT EXISTS (SELECT * FROM WorkflowProcessInstance  WHERE WorkflowProcessInstance.SchemeId = WorkflowProcessScheme.Id );

    SELECT COUNT(*) into st
    FROM WorkflowProcessInstance LEFT OUTER JOIN WorkflowProcessScheme ON WorkflowProcessInstance.SchemeId = WorkflowProcessScheme.Id
    WHERE WorkflowProcessScheme.Id IS NULL;

    RETURN st;
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
                340 AS Version,
                SYSDATE AS AppliedOn,
                'Migration340CreateFunction_DropUnusedWorkflowProcessScheme' AS Description
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
