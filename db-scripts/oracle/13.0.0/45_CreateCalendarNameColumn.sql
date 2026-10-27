DECLARE
  column_count NUMBER;
  index_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO column_count
  FROM USER_TAB_COLUMNS
  WHERE TABLE_NAME = 'WORKFLOWPROCESSINSTANCE'
    AND COLUMN_NAME = 'CALENDARNAME';

  IF column_count = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE WORKFLOWPROCESSINSTANCE ADD (CALENDARNAME NVARCHAR2(1024))';
  END IF;

  SELECT COUNT(*) INTO index_count
  FROM USER_INDEXES
  WHERE INDEX_NAME = 'IDX_CALENDARNAME';

  IF index_count = 0 THEN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_CALENDARNAME ON WORKFLOWPROCESSINSTANCE (CALENDARNAME) LOGGING';
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
                45 AS Version,
                SYSDATE AS AppliedOn,
                'Migration45CreateCalendarNameColumn' AS Description
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
