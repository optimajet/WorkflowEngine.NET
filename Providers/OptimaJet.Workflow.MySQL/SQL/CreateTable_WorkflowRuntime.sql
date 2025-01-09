CREATE TABLE IF NOT EXISTS `workflowruntime` (
    `RuntimeId` varchar(450) NOT NULL
    ,`Lock` binary(16) NOT NULL
    ,`Status` TINYINT(4) NOT NULL
    ,`RestorerId` varchar(450)
    ,`NextTimerTime` datetime(3)
    ,`NextServiceTimerTime` datetime(3)
    ,`LastAliveSignal` datetime(3),
    PRIMARY KEY (`RuntimeId`)
);