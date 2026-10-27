BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWAPPROVALHISTORY (
    ID RAW(16),
    PROCESSID RAW(16) NOT NULL,
    IDENTITYID NVARCHAR2(256),
    ALLOWEDTO CLOB NULL,
    TRANSITIONTIME TIMESTAMP NULL,
    SORT NUMBER(19) NULL,
    INITIALSTATE NVARCHAR2(1024) NOT NULL,
    DESTINATIONSTATE NVARCHAR2(1024) NOT NULL,
    TRIGGERNAME NVARCHAR2(1024) NULL,
    COMMENTARY CLOB NULL,
    CONSTRAINT PK_WORKFLOWAPPROVALHISTORY PRIMARY KEY (ID) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED )
)
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
                310 AS Version,
                SYSDATE AS AppliedOn,
                'Migration310CreateTable_WorkflowApprovalHistory' AS Description
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
