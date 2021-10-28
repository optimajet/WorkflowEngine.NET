ALTER TABLE `WorkflowProcessInstance` ADD `SubprocessName`  longtext NULL;

UPDATE `WorkflowProcessInstance` SET  `SubprocessName` = `StartingTransition`;

