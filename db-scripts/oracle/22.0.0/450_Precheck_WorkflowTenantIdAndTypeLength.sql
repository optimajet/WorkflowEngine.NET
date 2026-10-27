DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM WORKFLOWGLOBALPARAMETER
    WHERE LENGTH(TYPE) > 128;

    IF v_count > 0 THEN
        raise_application_error(-20000,
            'BREAKING CHANGES DETECTED: Some rows in the TYPE column in WORKFLOWGLOBALPARAMETER table are too long. Please contact support support@optimajet.com.');
    END IF;

    SELECT COUNT(*)
    INTO v_count
    FROM WORKFLOWPROCESSINSTANCE
    WHERE LENGTH(TENANTID) > 128;

    IF v_count > 0 THEN
        raise_application_error(-20001,
            'BREAKING CHANGES DETECTED: Some rows in the TENANTID column in WORKFLOWPROCESSINSTANCE table are too long. Please contact support support@optimajet.com.');
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
                450 AS Version,
                SYSDATE AS AppliedOn,
                'Migration450Precheck_WorkflowTenantIdAndTypeLength' AS Description
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
