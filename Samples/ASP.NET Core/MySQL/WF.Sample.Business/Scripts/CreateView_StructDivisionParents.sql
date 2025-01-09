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