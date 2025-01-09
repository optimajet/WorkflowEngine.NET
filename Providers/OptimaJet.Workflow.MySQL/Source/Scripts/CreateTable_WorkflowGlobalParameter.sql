CREATE TABLE IF NOT EXISTS `workflowglobalparameter` (
    `Id` binary(16) NOT NULL,
    `Type` varchar(512)  NOT NULL,
    `Name` varchar(256) NOT NULL,
    `Value` longtext NOT NULL,
    PRIMARY KEY  (`Id`)
);