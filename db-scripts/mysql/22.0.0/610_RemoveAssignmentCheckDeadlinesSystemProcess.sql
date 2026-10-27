DELETE FROM `workflowprocessinstancepersistence`
WHERE `ProcessId` = UNHEX('805A2DF0F4A24F468098C349CE2AEA7A');

DELETE FROM `workflowprocesstransitionhistory`
WHERE `ProcessId` = UNHEX('805A2DF0F4A24F468098C349CE2AEA7A');

DELETE FROM `workflowprocesstimer`
WHERE `ProcessId` = UNHEX('805A2DF0F4A24F468098C349CE2AEA7A');

DELETE FROM `workflowinbox`
WHERE `ProcessId` = UNHEX('805A2DF0F4A24F468098C349CE2AEA7A');

DELETE FROM `workflowapprovalhistory`
WHERE `ProcessId` = UNHEX('805A2DF0F4A24F468098C349CE2AEA7A');

DELETE FROM `workflowprocessinstancestatus`
WHERE `Id` = UNHEX('805A2DF0F4A24F468098C349CE2AEA7A');

DELETE FROM `workflowprocessinstance`
WHERE `Id` = UNHEX('805A2DF0F4A24F468098C349CE2AEA7A');

DELETE FROM `workflowprocessscheme`
WHERE `Id` = UNHEX('A1AFF77BD5AF2D48B0905A9CE449EB97')
   OR `SchemeCode` = 'OptimaJet.Workflow.Plugins.AssigmentPlugin.CheckDeadlines'
   OR `RootSchemeCode` = 'OptimaJet.Workflow.Plugins.AssigmentPlugin.CheckDeadlines';

-- DbScriptPublisher: mark migration as applied
SET @sql = (
    SELECT IF(
               EXISTS (
                   SELECT 1
                   FROM information_schema.tables
                   WHERE table_schema = DATABASE()
                     AND table_name = 'VersionInfo'
                     AND table_type = 'BASE TABLE'
               ),
               'INSERT INTO `VersionInfo` (`Version`, `AppliedOn`, `Description`)
                SELECT 610, CURRENT_TIMESTAMP, ''Migration610RemoveAssignmentCheckDeadlinesSystemProcess''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 610
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
