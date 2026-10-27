CREATE TABLE IF NOT EXISTS `workflowprocessassignment` (
    `Id` binary(16) NOT NULL,
    `AssignmentCode` varchar(256) NOT NULL,
    `ProcessId` binary(16) NOT NULL,
    `Name` varchar(256) NOT NULL,
    `Description` text,
    `StatusState` varchar(256) NOT NULL,
    `DateCreation` datetime(3) NOT NULL,
    `DateStart` datetime(3),
    `DateFinish` datetime(3),
    `DeadlineToStart` datetime(3),
    `DeadlineToComplete` datetime(3),
    `Executor` varchar(256) NOT NULL,
    `Observers` text,
    `Tags` text,
    `IsActive` bit(1) NOT NULL,
    `IsDeleted` bit(1) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `ProcessId` (`ProcessId`),
    KEY `AssignmentCode` (`AssignmentCode`),
    KEY `Executor` (`Executor`)
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
                SELECT 130, CURRENT_TIMESTAMP, ''Migration130CreateTable_WorkflowProcessAssignment''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 130
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
