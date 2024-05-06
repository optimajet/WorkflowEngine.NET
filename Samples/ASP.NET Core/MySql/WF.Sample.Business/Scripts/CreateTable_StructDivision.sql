CREATE TABLE IF NOT EXISTS `StructDivision`
(
    `Id` binary(16) NOT NULL,
    `Name` varchar(256) NOT NULL,
    `ParentId` binary(16) NULL,
    CONSTRAINT `PK_StructDivision` PRIMARY KEY (`Id`),
    FOREIGN KEY (`ParentId`) REFERENCES `StructDivision`(`Id`) ON DELETE CASCADE
);