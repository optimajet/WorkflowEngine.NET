BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE EMPLOYEE (
    ID RAW(16) NOT NULL,
    NAME NVARCHAR2(256) NOT NULL,
    STRUCTDIVISIONID RAW(16) NOT NULL,
    ISHEAD CHAR(1 BYTE) DEFAULT 0 NOT NULL,
    CONSTRAINT PK_EMPLOYEE PRIMARY KEY (ID) USING INDEX STORAGE (INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED),
    CONSTRAINT FK_EMPLOYEE_STRUCTDIVISION FOREIGN KEY (STRUCTDIVISIONID) REFERENCES STRUCTDIVISION(ID) ON DELETE CASCADE 
) LOGGING';
END;