BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE WORKFLOWINBOX (
    ID RAW(16),
    PROCESSID RAW(16) NOT NULL,
    IDENTITYID NVARCHAR2(256),
    ADDINGDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    AVAILABLECOMMANDS NVARCHAR2(1024) DEFAULT '''' NULL,
    CONSTRAINT PK_WORKFLOWINBOX PRIMARY KEY (ID) USING INDEX STORAGE ( INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED ))
    LOGGING';
END;