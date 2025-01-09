CREATE TABLE IF NOT EXISTS `EmployeeRole` (
    `EmployeeId` binary(16) NOT NULL,
    `RoleId` binary(16) NOT NULL,
    CONSTRAINT `PK_EmployeeRoles` PRIMARY KEY (`EmployeeId`, `RoleId`),
    FOREIGN KEY (`EmployeeId`) REFERENCES `Employee`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`) ON DELETE CASCADE
);