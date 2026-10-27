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

-- DbScriptPublisher: mark migration as applied
SET @sql = (
    SELECT IF(
               EXISTS (
                   SELECT 1
                   FROM information_schema.tables
                   WHERE table_schema = DATABASE()
                     AND table_name = 'VersionInfo'
                     AND table_type = 'BASE TABLE'
               ),
               'INSERT INTO `VersionInfo` (`Version`, `AppliedOn`, `Description`)
                SELECT 290, CURRENT_TIMESTAMP, ''Migration290CreateTable_WorkflowForms''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 290
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
