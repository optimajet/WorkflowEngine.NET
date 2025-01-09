CREATE TABLE IF NOT EXISTS `workflowinbox` (
    `Id` binary(16) NOT NULL,
    `ProcessId` binary(16) default NULL,
    `IdentityId` varchar(256) default NULL,
    `AddingDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `AvailableCommands` varchar(1024) DEFAULT '',
    PRIMARY KEY  (`Id`),
    KEY `ProcessId` (`ProcessId`),
    KEY `IdentityId` (`IdentityId`)
);