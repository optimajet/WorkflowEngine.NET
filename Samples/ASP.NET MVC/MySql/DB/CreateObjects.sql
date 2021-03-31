/*
Company: OptimaJet
Project: WF.Sample WorkflowEngine.NET
File: CreateObjects.sql
*/


CREATE TABLE IF NOT EXISTS `StructDivision`
(
	  `Id` binary(16) NOT NULL,
	  `Name` varchar(256) NOT NULL,
	  `ParentId` binary(16) NULL,
	  CONSTRAINT `PK_StructDivision` PRIMARY KEY (`Id`),
      FOREIGN KEY (`ParentId`) REFERENCES `StructDivision`(`Id`) ON DELETE CASCADE
);


CREATE OR REPLACE VIEW `vStructDivisionParents`
	AS
	WITH RECURSIVE `cteRecursive` as (
	 select sd.`Id` AS `FirstId`, sd.`ParentId` as `ParentId`, sd.`Id` AS `Id`
	  from  `StructDivision` AS sd WHERE sd.`ParentId` IS NOT NULL
	 union all 
	 select r.`FirstId` AS `FirstId`, sdr.`ParentId` AS `ParentId`, sdr.`Id` AS `Id`
	 from `StructDivision` AS sdr
	 inner join `cteRecursive` AS r ON r.`ParentId` = sdr.`Id`)
	select DISTINCT `FirstId` AS `Id`, `ParentId` AS `ParentId` FROM `cteRecursive`; 

CREATE OR REPLACE VIEW `vStructDivisionParentsAndThis`
	AS
	select  `Id` AS `Id`, `Id` AS `ParentId` FROM `StructDivision`
	UNION 
	select  `Id` AS `Id`, `ParentId` AS `ParentId` FROM `vStructDivisionParents`;

CREATE TABLE IF NOT EXISTS `Employee` (
	  `Id` binary(16) NOT NULL,
	  `Name` varchar(256) NOT NULL,
	  `StructDivisionId` binary(16) NOT NULL,
	  `IsHead` boolean NOT NULL DEFAULT 0,
	  CONSTRAINT `PK_Employee` PRIMARY KEY (`Id`),
      FOREIGN KEY (`StructDivisionId`) REFERENCES `StructDivision`(`Id`) ON DELETE CASCADE
);

CREATE OR REPLACE VIEW `vHeads`
	AS
	select  e.`Id` AS `Id`, e.`Name` AS `Name`, eh.`Id` AS `HeadId`, eh.`Name` AS `HeadName` FROM `Employee` AS e 
		INNER JOIN `vStructDivisionParentsAndThis` AS vsp ON e.`StructDivisionId` = vsp.`Id`
		INNER JOIN `Employee` AS eh ON eh.`StructDivisionId` = vsp.`ParentId` AND eh.`IsHead` = true;


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

CREATE TABLE IF NOT EXISTS `Roles` (
	  `Id` binary(16) NOT NULL,
	  `Name` varchar(256) NOT NULL,
	  CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`)
	);

CREATE TABLE IF NOT EXISTS `EmployeeRole` (
	  `EmployeeId` binary(16) NOT NULL,
	  `RoleId` binary(16) NOT NULL,
	  CONSTRAINT `PK_EmployeeRoles` PRIMARY KEY (`EmployeeId`, `RoleId`),
      FOREIGN KEY (`EmployeeId`) REFERENCES `Employee`(`Id`) ON DELETE CASCADE,
      FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`) ON DELETE CASCADE
);