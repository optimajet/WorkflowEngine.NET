CREATE TABLE IF NOT EXISTS `workflowapprovalhistory`
(
    `Id`               binary(16)    NOT NULL,
    `ProcessId`        binary(16)    NOT NULL,
    `IdentityId`       varchar(256)  NULL,
    `AllowedTo`        longtext      NULL,
    `TransitionTime`   datetime      NULL,
    `Sort`             bigint        NULL,
    `InitialState`     varchar(1024) NOT NULL,
    `DestinationState` varchar(1024) NOT NULL,
    `TriggerName`      varchar(1024) NULL,
    `Commentary`       longtext      NULL,
    PRIMARY KEY (`Id`),
    KEY `ProcessId` (`ProcessId`),
    KEY `IdentityId` (`IdentityId`)
);