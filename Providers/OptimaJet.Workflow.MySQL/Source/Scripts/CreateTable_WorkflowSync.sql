CREATE TABLE IF NOT EXISTS `workflowsync` (
    `Name` varchar(450) NOT NULL,
    `Lock` binary(16) NOT NULL,
     PRIMARY KEY (`Name`)
);