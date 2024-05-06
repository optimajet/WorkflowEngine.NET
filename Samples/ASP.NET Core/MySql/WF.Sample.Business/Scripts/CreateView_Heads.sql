CREATE OR REPLACE VIEW `vHeads`
AS
select  e.`Id` AS `Id`, e.`Name` AS `Name`, eh.`Id` AS `HeadId`, eh.`Name` AS `HeadName` FROM `Employee` AS e
    INNER JOIN `vStructDivisionParentsAndThis` AS vsp ON e.`StructDivisionId` = vsp.`Id`
    INNER JOIN `Employee` AS eh ON eh.`StructDivisionId` = vsp.`ParentId` AND eh.`IsHead` = true;