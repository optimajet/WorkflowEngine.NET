CREATE TABLE IF NOT EXISTS `workflowprocessinstancestatus` (
    `Id` binary(16) NOT NULL,
    `Status` tinyint(4) NOT NULL,
    `Lock` binary(16) NOT NULL,
    `RuntimeId` varchar (450) NOT NULL,
    `SetTime` datetime NOT NULL,
    PRIMARY KEY  (`Id`),
    KEY `Status` (`Status`)
);