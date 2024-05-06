CREATE OR REPLACE FUNCTION DropUnusedWorkflowProcessScheme
    RETURN NUMBER
    IS
    st NUMBER := 0;
BEGIN
    DELETE FROM WorkflowProcessScheme
    WHERE WorkflowProcessScheme.IsObsolete = 1
      AND NOT EXISTS (SELECT * FROM WorkflowProcessInstance  WHERE WorkflowProcessInstance.SchemeId = WorkflowProcessScheme.Id );

    SELECT COUNT(*) into st
    FROM WorkflowProcessInstance LEFT OUTER JOIN WorkflowProcessScheme ON WorkflowProcessInstance.SchemeId = WorkflowProcessScheme.Id
    WHERE WorkflowProcessScheme.Id IS NULL;

    RETURN st;
END;