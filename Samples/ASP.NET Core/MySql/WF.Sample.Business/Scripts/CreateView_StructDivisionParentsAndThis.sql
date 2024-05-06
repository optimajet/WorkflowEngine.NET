CREATE OR REPLACE VIEW `vStructDivisionParentsAndThis`
AS
select  `Id` AS `Id`, `Id` AS `ParentId` FROM `StructDivision`
UNION
select  `Id` AS `Id`, `ParentId` AS `ParentId` FROM `vStructDivisionParents`;