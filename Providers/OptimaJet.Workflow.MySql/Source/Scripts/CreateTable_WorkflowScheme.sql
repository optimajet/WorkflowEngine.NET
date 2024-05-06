CREATE TABLE IF NOT EXISTS `workflowscheme` (
    `Code` varchar(128) NOT NULL,
    `Scheme` longtext NOT NULL,
    `CanBeInlined` bit(1) NOT NULL default 0,
    `InlinedSchemes` varchar(1024) NULL,
    `Tags` varchar(1024) NULL,
    PRIMARY KEY  (`Code`)
);