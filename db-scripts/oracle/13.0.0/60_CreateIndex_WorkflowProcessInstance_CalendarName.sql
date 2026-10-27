DECLARE
v_index_count PLS_INTEGER;
BEGIN
SELECT COUNT(*)
INTO v_index_count
FROM ALL_INDEXES
WHERE OWNER = SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA')
  AND INDEX_NAME = 'IDX_CALENDARNAME';

IF v_index_count = 0 THEN
        EXECUTE IMMEDIATE
            'CREATE INDEX IDX_CALENDARNAME ON WORKFLOWPROCESSINSTANCE (CALENDARNAME) LOGGING';
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
                60 AS Version,
                SYSDATE AS AppliedOn,
                'Migration60CreateIndex_WorkflowProcessInstance_CalendarName' AS Description
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
