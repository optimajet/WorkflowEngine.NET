BEGIN
EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX IDX_WORKFLOWPROCESSINSTANCEP_PP ON WORKFLOWPROCESSINSTANCEP (PROCESSID, PARAMETERNAME)';
END;