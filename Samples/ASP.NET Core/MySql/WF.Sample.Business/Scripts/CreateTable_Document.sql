CREATE TABLE IF NOT EXISTS `Document` (
    `Id` binary(16) NOT NULL,
    `Number` SERIAL,
    `Name` varchar(256) NOT NULL,
    `Comment` longtext NULL,
    `AuthorId` binary(16) NOT NULL,
    `ManagerId` binary(16) NULL,
    `Sum` decimal(13, 4) NOT NULL DEFAULT 0,
    `State` varchar(1024) NOT NULL DEFAULT 'VacationRequestCreated',
    `StateName` varchar(1024) ,
    CONSTRAINT `PK_Document` PRIMARY KEY (`Id`),
    FOREIGN KEY (`AuthorId`) REFERENCES `Employee`(`Id`),
    FOREIGN KEY (`ManagerId`) REFERENCES `Employee`(`Id`)
);