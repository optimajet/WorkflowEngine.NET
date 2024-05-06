CREATE TABLE IF NOT EXISTS `workflowprocessinstancepersistence` (
    `Id` binary(16) NOT NULL,
    `ProcessId` binary(16) NOT NULL,
    `ParameterName` varchar(256) NOT NULL,
    `Value` longtext NOT NULL,
    PRIMARY KEY  (`Id`),
    KEY `ProcessId` (`ProcessId`)
);