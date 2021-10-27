ALTER TABLE `workflowprocessinstance` ADD `CreationDate`  datetime NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE `workflowprocessinstance` ADD `LastTransitionDate` datetime NULL;

ALTER TABLE `workflowprocesstransitionhistory` ADD `StartTransitionTime`  datetime;
ALTER TABLE `workflowprocesstransitionhistory` ADD `TransitionDuration` bigint;

ALTER TABLE `workflowinbox` ADD `AddingDate`  datetime NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE `workflowinbox` ADD `AvailableCommands` varchar(1024) DEFAULT '';


set @exist := (select count(*) from information_schema.statistics where table_name = 'workflowapprovalhistory'
                                                                        and index_name = 'ix_workflowprocessinstance_identityid' and table_schema = database());
set @sqlstmt := if( @exist <= 0, 'CREATE INDEX `ix_workflowprocessinstance_identityid` ON `workflowapprovalhistory` (`IdentityId`)',
                    'select ''INFO: Index already exists.''');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;