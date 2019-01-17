/*
Company: OptimaJet
Project: WorkflowEngine.NET Provider for MySql
Version: 3.1
File: update_3_1.sql
*/

-- WorkflowProcessScheme
ALTER TABLE `workflowprocessscheme` MODIFY  COLUMN `DefiningParametersHash` varchar(24) NOT NULL;


set @exist := (select count(*) from information_schema.statistics where table_name = 'workflowprocessscheme'
                                                                        and index_name = 'ix_workflowprocessscheme_schemecode_hash_isobsolete' and table_schema = database());
set @sqlstmt := if( @exist <= 0, 'CREATE INDEX `ix_workflowprocessscheme_schemecode_hash_isobsolete` ON `workflowprocessscheme` (`SchemeCode`,`DefiningParametersHash`,`IsObsolete`)',
                    'select ''INFO: Index already exists.''');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;


-- WorkflowGlobalParameter

ALTER TABLE `workflowglobalparameter` MODIFY  COLUMN `Type` varchar(512) NOT NULL;

set @exist := (select count(*) from information_schema.statistics where table_name = 'workflowglobalparameter'
                                                                        and index_name = 'ix_workflowglobalparameter_type_name' and table_schema = database());
set @sqlstmt := if( @exist <= 0, 'CREATE INDEX `ix_workflowglobalparameter_type_name` ON `workflowglobalparameter` (`Type`,`Name`)',
                    'select ''INFO: Index already exists.''');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;



