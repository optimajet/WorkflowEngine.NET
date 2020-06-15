/*
Company: OptimaJet
Project: WF.Sample WorkflowEngine.NET
File: CreateObjects.sql
*/


CREATE TABLE STRUCTDIVISION
(
	  ID RAW(16),
	  NAME NVARCHAR2(256) NOT NULL,
	  PARENTID RAW(16) NULL,
	  CONSTRAINT PK_STRUCTDIVISION PRIMARY KEY (ID) USING INDEX STORAGE (INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED),
      CONSTRAINT FK_STRUCTDIVISION_STRUCTDIVISION FOREIGN KEY (PARENTID) REFERENCES STRUCTDIVISION(ID) ON DELETE CASCADE
) LOGGING;


CREATE OR REPLACE VIEW VSTRUCTDIVISIONPARENTS AS
	WITH CTERECURSIVE(FIRSTID, PARENTID, ID) AS (
	 SELECT SD.ID AS FIRSTID, SD.PARENTID AS PARENTID, SD.ID AS ID
	  FROM  STRUCTDIVISION SD WHERE SD.PARENTID IS NOT NULL
	 UNION ALL 
	 SELECT R.FIRSTID AS FIRSTID, SDR.PARENTID AS PARENTID, SDR.ID AS ID
	 FROM STRUCTDIVISION SDR
	 INNER JOIN CTERECURSIVE R ON R.PARENTID = SDR.ID)
	SELECT DISTINCT FIRSTID AS ID, PARENTID AS PARENTID FROM CTERECURSIVE; 

CREATE OR REPLACE VIEW VSTRUCTDIVISIONPARENTSANDTHIS
	AS
	SELECT  ID AS ID, ID AS PARENTID FROM STRUCTDIVISION
	UNION 
	SELECT  ID AS ID, PARENTID AS PARENTID FROM VSTRUCTDIVISIONPARENTS;

CREATE TABLE EMPLOYEE (
	  ID RAW(16) NOT NULL,
	  NAME NVARCHAR2(256) NOT NULL,
	  STRUCTDIVISIONID RAW(16) NOT NULL,
	  ISHEAD CHAR(1 BYTE) DEFAULT 0 NOT NULL,
	  CONSTRAINT PK_EMPLOYEE PRIMARY KEY (ID) USING INDEX STORAGE (INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED),
      CONSTRAINT FK_EMPLOYEE_STRUCTDIVISION FOREIGN KEY (STRUCTDIVISIONID) REFERENCES STRUCTDIVISION(ID) ON DELETE CASCADE 
	) LOGGING;

CREATE OR REPLACE VIEW VHEADS
	AS
	SELECT  E.ID AS ID, E.NAME AS NAME, EH.ID AS HEADID, EH.NAME AS HEADNAME FROM EMPLOYEE E 
		INNER JOIN VSTRUCTDIVISIONPARENTSANDTHIS VSP ON E.STRUCTDIVISIONID = VSP.ID
		INNER JOIN EMPLOYEE EH ON EH.STRUCTDIVISIONID = VSP.PARENTID AND EH.ISHEAD = 1;

CREATE TABLE DOCUMENT (
	  ID RAW(16) NOT NULL,
	  "Number" NUMBER GENERATED ALWAYS AS IDENTITY,
	  NAME NVARCHAR2(256) NOT NULL,
	  "Comment" NCLOB NULL,
	  AUTHORID RAW(16) NOT NULL,
	  MANAGERID RAW(16) NULL,
	  SUM NUMBER(19,4) DEFAULT 0 NOT NULL,
	  STATE NVARCHAR2(1024) DEFAULT 'VacationRequestCreated' NOT NULL,
	  STATENAME NVARCHAR2(1024) ,
	  CONSTRAINT PK_DOCUMENT PRIMARY KEY (ID) USING INDEX STORAGE (INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED),
      CONSTRAINT FK_DOCUMENT_AUTHORID FOREIGN KEY (AUTHORID) REFERENCES EMPLOYEE(ID),
      CONSTRAINT FK_DOCUMENT_MANAGERID FOREIGN KEY (MANAGERID) REFERENCES EMPLOYEE(ID)
      ) LOGGING;

CREATE TABLE DOCUMENTTRANSITIONHISTORY (
	  ID RAW(16) NOT NULL,
	  DOCUMENTID RAW(16) NOT NULL,
	  EMPLOYEEID RAW(16) NULL,
	  ALLOWEDTOEMPLOYEENAMES NCLOB NOT NULL,
	  TRANSITIONTIME DATE NULL,
	  "Order" NUMBER GENERATED ALWAYS AS IDENTITY,
	  INITIALSTATE NVARCHAR2(1024) NOT NULL,
	  DESTINATIONSTATE NVARCHAR2(1024) NOT NULL,
	  COMMAND NVARCHAR2(1024) NOT NULL,
	  CONSTRAINT PK_DOCUMENTTRANSITIONHISTORY PRIMARY KEY (ID) USING INDEX STORAGE (INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED),
      CONSTRAINT FK_DOCUMENTTRANSITIONHISTORY_DOCUMENT FOREIGN KEY (DOCUMENTID) REFERENCES DOCUMENT(ID) ON DELETE CASCADE,
      CONSTRAINT FK_DOCUMENTTRANSITIONHISTORY_EMPLOYEE FOREIGN KEY (EMPLOYEEID) REFERENCES EMPLOYEE(ID)
	) LOGGING;

CREATE TABLE ROLES (
	  ID RAW(16) NOT NULL,
	  NAME NVARCHAR2(256) NOT NULL,
	  CONSTRAINT PK_ROLES PRIMARY KEY (ID) USING INDEX STORAGE (INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED)
	) LOGGING;

CREATE TABLE EMPLOYEEROLE (
	  EMPLOYEEID RAW(16) NOT NULL,
	  ROLEID RAW(16) NOT NULL,
	  CONSTRAINT PK_EMPLOYEEROLES PRIMARY KEY (EMPLOYEEID, ROLEID) 
        USING INDEX STORAGE (INITIAL 64K NEXT 1M MAXEXTENTS UNLIMITED),
      CONSTRAINT FK_EMPLOYEEROLE_EMPLOYEE FOREIGN KEY (EMPLOYEEID) REFERENCES EMPLOYEE(ID) ON DELETE CASCADE,
      CONSTRAINT FK_EMPLOYEEROLE_ROLES FOREIGN KEY (ROLEID) REFERENCES ROLES(ID) ON DELETE CASCADE
) LOGGING;