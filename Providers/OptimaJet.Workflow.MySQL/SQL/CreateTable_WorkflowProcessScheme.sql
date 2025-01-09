CREATE TABLE IF NOT EXISTS `workflowprocessscheme` (
    `Id` binary(16) NOT NULL,
    `Scheme` longtext NOT NULL,
    `DefiningParameters` longtext NOT NULL,
    `DefiningParametersHash` varchar(24) NOT NULL,
    `SchemeCode` varchar(256) NOT NULL,
    `IsObsolete` bit(1) NOT NULL,
    `RootSchemeCode` varchar(256) NULL,
    `RootSchemeId` binary(16) NULL,
    `AllowedActivities` longtext NULL,
    `StartingTransition` longtext NULL,
    PRIMARY KEY  (`Id`)
);