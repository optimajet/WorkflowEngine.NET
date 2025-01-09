CREATE TABLE IF NOT EXISTS `Roles` (
    `Id` binary(16) NOT NULL,
    `Name` varchar(256) NOT NULL,
    CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`)
);