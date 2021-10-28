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

CREATE INDEX `ix_workflowprocessassignment_processid_executor` ON `workflowprocessassignment` (`ProcessId`,`Executor`);


set @exist := (select count(*) from information_schema.statistics where table_name = 'workflowprocessassignment'
                                                                        and index_name = 'ix_workflowprocessassignment_processid_executo' and table_schema = database());
set @sqlstmt := if( @exist <= 0, 'CREATE INDEX `ix_workflowprocessassignment_processid_executor` ON `workflowprocessassignment` (`ProcessId`,`Executor`)',
                    'select ''INFO: Index already exists.''');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;

ALTER TABLE `workflowprocesstransitionhistory` ADD `ActorName`  varchar(256) default NULL;
ALTER TABLE `workflowprocesstransitionhistory` ADD `ExecutorName` varchar(256) default NULL;


DROP FUNCTION IF EXISTS `DropUnusedWorkflowProcessScheme`;

DELIMITER $$
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
END$$
DELIMITER ;
