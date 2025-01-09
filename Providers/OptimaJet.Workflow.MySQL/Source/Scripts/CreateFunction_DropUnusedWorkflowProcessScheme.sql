DROP FUNCTION IF EXISTS `DropUnusedWorkflowProcessScheme`;

CREATE FUNCTION `DropUnusedWorkflowProcessScheme`()
    RETURNS INTEGER
    DETERMINISTIC
BEGIN
    DECLARE st INTEGER;

DELETE FROM `workflowprocessscheme` AS wps
WHERE wps.`IsObsolete` = 1
  AND NOT EXISTS (SELECT * FROM `workflowprocessinstance` wpi WHERE wpi.`SchemeId` = wps.`Id` );

SELECT COUNT(*) into st
FROM `workflowprocessinstance` wpi LEFT OUTER JOIN `workflowprocessscheme` wps ON wpi.`SchemeId` = wps.`Id`
WHERE wps.`Id` IS NULL;

RETURN st;
END;