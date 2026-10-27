DECLARE
    v_table_count NUMBER;
    v_has_rows NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_table_count
    FROM USER_TABLES
    WHERE TABLE_NAME = 'WORKFLOWPROCESSASSIGNMENT';

    IF v_table_count > 0 THEN
        EXECUTE IMMEDIATE 'SELECT COUNT(*) FROM WORKFLOWPROCESSASSIGNMENT WHERE ROWNUM = 1'
            INTO v_has_rows;

        IF v_has_rows = 0 THEN
            EXECUTE IMMEDIATE 'DROP TABLE WORKFLOWPROCESSASSIGNMENT';
        END IF;
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
                720 AS Version,
                SYSDATE AS AppliedOn,
                'Migration720DropEmptyWorkflowProcessAssignmentTable' AS Description
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
