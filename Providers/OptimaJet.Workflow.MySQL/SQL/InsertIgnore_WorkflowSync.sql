INSERT IGNORE INTO `workflowsync`(`Name`,`Lock`) VALUES ('Timer', UUID_TO_BIN(UUID()));
INSERT IGNORE INTO `workflowsync`(`Name`,`Lock`) VALUES ('ServiceTimer', UUID_TO_BIN(UUID()));