BEGIN
EXECUTE IMMEDIATE '
CREATE TABLE WORKFLOWFORM (
                              ID RAW(16) NOT NULL,
                              NAME NVARCHAR2(512) NOT NULL,
                              VERSION NUMBER(10) NOT NULL,
                              CREATIONDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                              UPDATEDDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                              DEFINITION NCLOB NOT NULL,
                              LOCKFLAG NUMBER(10) NOT NULL,
                              CONSTRAINT PK_WORKFLOWFORM PRIMARY KEY (ID),
                              CONSTRAINT UQ_WORKFLOWFORM_NAME_VERSION UNIQUE (NAME, VERSION)
)
    LOGGING';
END;
