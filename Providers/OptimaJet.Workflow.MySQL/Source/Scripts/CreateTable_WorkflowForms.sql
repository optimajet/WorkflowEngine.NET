create table if not exists `workflowform` (
    `Id` binary(16) not null,
    `Name` varchar(512) not null,
    `Version` int not null,
    `CreationDate` datetime not null default current_timestamp,
    `UpdatedDate` datetime not null default current_timestamp,
    `Definition` longtext not null,
    `Lock` int not null,
    primary key (`Id`),
    unique key `ix_workflowform_name_version` (`Name`, `Version`)
    );