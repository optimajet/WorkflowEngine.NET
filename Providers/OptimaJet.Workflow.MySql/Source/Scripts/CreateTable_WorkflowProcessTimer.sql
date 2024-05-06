CREATE TABLE IF NOT EXISTS `workflowprocesstimer` (
    `Id` binary(16) NOT NULL,
    `ProcessId` binary(16) NOT NULL,
    `RootProcessId` binary(16) NOT NULL,
    `Name` varchar(256) NOT NULL,
    `NextExecutionDateTime` datetime(3) NOT NULL,
    `Ignore` bit(1) NOT NULL,
    PRIMARY KEY  (`Id`),
    KEY `ProcessId` (`ProcessId`),
    KEY `Name` (`Name`),
    KEY `NextExecutionDateTime` (`NextExecutionDateTime`)
);