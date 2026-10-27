UPDATE `workflowprocessinstancestatus` target
INNER JOIN `workflowprocessinstance` source ON source.`Id` = target.`Id`
SET target.`TenantId` = source.`TenantId`
WHERE source.`TenantId` IS NOT NULL
  AND (target.`TenantId` IS NULL OR target.`TenantId` <> source.`TenantId`);

UPDATE `workflowapprovalhistory` target
INNER JOIN `workflowprocessinstance` source ON source.`Id` = target.`ProcessId`
SET target.`TenantId` = source.`TenantId`
WHERE source.`TenantId` IS NOT NULL
  AND (target.`TenantId` IS NULL OR target.`TenantId` <> source.`TenantId`);

UPDATE `workflowinbox` target
INNER JOIN `workflowprocessinstance` source ON source.`Id` = target.`ProcessId`
SET target.`TenantId` = source.`TenantId`
WHERE source.`TenantId` IS NOT NULL
  AND (target.`TenantId` IS NULL OR target.`TenantId` <> source.`TenantId`);

UPDATE `workflowprocessinstancepersistence` target
INNER JOIN `workflowprocessinstance` source ON source.`Id` = target.`ProcessId`
SET target.`TenantId` = source.`TenantId`
WHERE source.`TenantId` IS NOT NULL
  AND (target.`TenantId` IS NULL OR target.`TenantId` <> source.`TenantId`);

UPDATE `workflowprocesstimer` target
INNER JOIN `workflowprocessinstance` source ON source.`Id` = target.`ProcessId`
SET target.`TenantId` = source.`TenantId`
WHERE source.`TenantId` IS NOT NULL
  AND (target.`TenantId` IS NULL OR target.`TenantId` <> source.`TenantId`);

UPDATE `workflowprocesstransitionhistory` target
INNER JOIN `workflowprocessinstance` source ON source.`Id` = target.`ProcessId`
SET target.`TenantId` = source.`TenantId`
WHERE source.`TenantId` IS NOT NULL
  AND (target.`TenantId` IS NULL OR target.`TenantId` <> source.`TenantId`);

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
                SELECT 580, CURRENT_TIMESTAMP, ''Migration580Update_TenantId_Synchronization''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 580
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
