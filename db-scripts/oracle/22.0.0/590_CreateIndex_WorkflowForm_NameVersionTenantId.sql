BEGIN
EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UQ_WORKFLOWFORM_NAME_VERSION_TENANTID ON WORKFLOWFORM (NAME, VERSION, CASE WHEN TENANTID IS NULL THEN 0 ELSE 1 END, NVL(TENANTID, N''__NULL__''))';
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
                590 AS Version,
                SYSDATE AS AppliedOn,
                'Migration590CreateIndex_WorkflowForm_NameVersionTenantId' AS Description
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
