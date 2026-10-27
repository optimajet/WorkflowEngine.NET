CREATE TABLE IF NOT EXISTS `workflowruntime` (
    `RuntimeId` varchar(450) NOT NULL
    ,`Lock` binary(16) NOT NULL
    ,`Status` TINYINT(4) NOT NULL
    ,`RestorerId` varchar(450)
    ,`NextTimerTime` datetime(3)
    ,`NextServiceTimerTime` datetime(3)
    ,`LastAliveSignal` datetime(3),
    PRIMARY KEY (`RuntimeId`)
);

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
                SELECT 190, CURRENT_TIMESTAMP, ''Migration190CreateTable_WorkflowRuntime''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 190
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
