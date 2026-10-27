BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWPROCESSSCHEME (
    ID RAW(16),
    SCHEME CLOB NOT NULL,
    DEFININGPARAMETERS CLOB NOT NULL,
    DEFININGPARAMETERSHASH NVARCHAR2(24) NOT NULL,
    SCHEMECODE NVARCHAR2(256) NOT NULL,
    ISOBSOLETE CHAR(1 BYTE) DEFAULT 0 NOT NULL,
    ROOTSCHEMECODE NVARCHAR2(256) NULL,
    ROOTSCHEMEID RAW(16) NULL,
    ALLOWEDACTIVITIES CLOB NULL,
    STARTINGTRANSITION CLOB NULL,
    CONSTRAINT PK_WORKFLOWPROCESSSCHEME PRIMARY KEY (ID) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED ))
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
                70 AS Version,
                SYSDATE AS AppliedOn,
                'Migration70CreateTable_WorkflowProcessScheme' AS Description
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
