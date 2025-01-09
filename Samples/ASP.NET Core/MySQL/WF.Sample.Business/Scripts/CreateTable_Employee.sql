CREATE TABLE IF NOT EXISTS `Employee` (
    `Id` binary(16) NOT NULL,
    `Name` varchar(256) NOT NULL,
    `StructDivisionId` binary(16) NOT NULL,
    `IsHead` boolean NOT NULL DEFAULT 0,
    CONSTRAINT `PK_Employee` PRIMARY KEY (`Id`),
    FOREIGN KEY (`StructDivisionId`) REFERENCES `StructDivision`(`Id`) ON DELETE CASCADE
);