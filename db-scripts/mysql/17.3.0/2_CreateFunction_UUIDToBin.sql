CREATE FUNCTION IF NOT EXISTS UUID_TO_BIN(uuid CHAR(36))
    RETURNS BINARY(16)
    DETERMINISTIC
    NO SQL
BEGIN
RETURN UNHEX(REPLACE(uuid, '-', ''));
END;

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
                SELECT 2, CURRENT_TIMESTAMP, ''Migration2CreateFunction_UUIDToBin''
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM `VersionInfo`
                    WHERE `Version` = 2
                )',
               'DO 0'
           )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
