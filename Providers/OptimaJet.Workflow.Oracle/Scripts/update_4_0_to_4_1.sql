/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for Oracle
Version: 4.1
File: update_4_0_to_4_1.sql
*/

ALTER TABLE WORKFLOWPROCESSINSTANCE ADD
(
	TENANTID NVARCHAR2(1024) NULL
);

ALTER TABLE WORKFLOWSCHEME ADD
(
    TAGS NVARCHAR2(1024) NULL
);
