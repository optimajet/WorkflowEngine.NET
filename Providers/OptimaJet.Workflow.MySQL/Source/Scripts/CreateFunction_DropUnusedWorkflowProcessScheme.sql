DROP FUNCTION IF EXISTS `DropUnusedWorkflowProcessScheme`;

CREATE FUNCTION `DropUnusedWorkflowProcessScheme`()
    RETURNS INTEGER
    DETERMINISTIC
BEGIN
    DECLARE st INTEGER;

DELETE FROM `workflowprocessscheme`
WHERE `IsObsolete` = 1
  AND NOT EXISTS (
    SELECT 1 FROM `workflowprocessinstance`
    WHERE `workflowprocessinstance`.`SchemeId` = `workflowprocessscheme`.`Id`
);

SELECT COUNT(*) INTO st
FROM `workflowprocessinstance`
         LEFT JOIN `workflowprocessscheme` ON `workflowprocessinstance`.`SchemeId` = `workflowprocessscheme`.`Id`
WHERE `workflowprocessscheme`.`Id` IS NULL;

RETURN st;
END