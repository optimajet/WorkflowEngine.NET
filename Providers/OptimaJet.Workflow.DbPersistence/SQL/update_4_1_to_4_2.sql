IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstanceStatus' 
AND [COLUMN_NAME] = N'RuntimeId')
BEGIN
	ALTER TABLE WorkflowProcessInstanceStatus ADD [RuntimeId] nvarchar(450) NULL;
	PRINT 'ADD WorkflowProcessInstanceStatus.RuntimeId type nvarchar(450) NULL';

	EXEC('UPDATE WorkflowProcessInstanceStatus SET RuntimeId = ''00000000-0000-0000-0000-000000000000''');

	ALTER TABLE WorkflowProcessInstanceStatus ALTER COLUMN [RuntimeId] nvarchar(450) NOT NULL;

	/****** Object:  Index [IX_WorkflowProcessInstanceStatus_Status]    Script Date: 12.03.2020 20:18:45 ******/
	CREATE NONCLUSTERED INDEX [IX_WorkflowProcessInstanceStatus_Status] ON [dbo].[WorkflowProcessInstanceStatus]
	(
		[Status] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];


	/****** Object:  Index [IX_WorkflowProcessInstanceStatus_Status_Runtime]    Script Date: 12.03.2020 20:21:11 ******/
	CREATE NONCLUSTERED INDEX [IX_WorkflowProcessInstanceStatus_Status_Runtime] ON [dbo].[WorkflowProcessInstanceStatus]
	(
		[Status] ASC,
		[RuntimeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];


END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstanceStatus' 
AND [COLUMN_NAME] = N'SetTime')
BEGIN
	ALTER TABLE WorkflowProcessInstanceStatus ADD [SetTime] datetime NULL;
	PRINT 'ADD WorkflowProcessInstanceStatus.SetTime type datetime NULL';

	EXEC('UPDATE WorkflowProcessInstanceStatus SET SetTime = GETDATE()');


	ALTER TABLE WorkflowProcessInstanceStatus ALTER COLUMN [SetTime] datetime NOT NULL;

END
GO

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowRuntime'
		)
BEGIN
	CREATE TABLE WorkflowRuntime (
		[RuntimeId] nvarchar(450) NOT NULL CONSTRAINT PK_WorkflowRuntime PRIMARY KEY
		,[Lock] UNIQUEIDENTIFIER NOT NULL
		,[Status] TINYINT NOT NULL
		,[RestorerId] nvarchar(450)
		,[NextTimerTime] datetime
		,[NextServiceTimerTime] datetime
		,[LastAliveSignal] datetime
		)

	PRINT 'WorkflowRuntime CREATE TABLE'

	EXEC('INSERT INTO WorkflowRuntime  (RuntimeId,Lock,Status)  VALUES (''00000000-0000-0000-0000-000000000000'', NEWID(),100)');
END
GO

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowSync'
		)
BEGIN
	CREATE TABLE WorkflowSync (
		[Name] nvarchar(450) NOT NULL CONSTRAINT PK_WorkflowSync PRIMARY KEY
		,[Lock] UNIQUEIDENTIFIER NOT NULL
		)

	INSERT INTO [dbo].[WorkflowSync]
           ([Name]
           ,[Lock])
     VALUES
           ('Timer',
           NEWID());

	INSERT INTO [dbo].[WorkflowSync]
           ([Name]
           ,[Lock])
     VALUES
           ('ServiceTimer',
           NEWID());

	PRINT 'WorkflowSync CREATE TABLE'
END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessTimer'
AND [COLUMN_NAME] = N'RootProcessId')
BEGIN
	ALTER TABLE WorkflowProcessTimer ADD [RootProcessId] UNIQUEIDENTIFIER NULL;


	EXEC('UPDATE WorkflowProcessTimer SET RootProcessId = (SELECT [RootProcessId] FROM WorkflowProcessInstance wpi WHERE wpi.[Id] = [ProcessId])');

	ALTER TABLE WorkflowProcessTimer ALTER COLUMN [RootProcessId] UNIQUEIDENTIFIER NOT NULL;

    PRINT 'ADD WorkflowProcessTimer.RootProcessId type UNIQUEIDENTIFIER NOT NULL';

END
GO

IF NOT EXISTS (
        SELECT 1
        FROM [INFORMATION_SCHEMA].[TABLES]
        WHERE [TABLE_NAME] = N'WorkflowApprovalHistory'
    )
    BEGIN
        CREATE TABLE [dbo].[WorkflowApprovalHistory](
            [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowApprovalHistory PRIMARY KEY NONCLUSTERED
            ,[ProcessId] UNIQUEIDENTIFIER NOT NULL
            ,[IdentityId] NVARCHAR(1024) NULL
            ,[AllowedTo] NVARCHAR(max) NULL
            ,[TransitionTime] DateTime NULL
            ,[Sort] BIGINT NULL
            ,[InitialState] NVARCHAR(1024) NOT NULL
            ,[DestinationState] NVARCHAR(1024) NOT NULL
            ,[TriggerName] NVARCHAR(1024) NULL
            ,[Commentary] NVARCHAR(max) NULL
        )

        CREATE CLUSTERED INDEX IX_ProcessId_Clustered ON WorkflowApprovalHistory (ProcessId)
        PRINT 'WorkflowApprovalHistory CREATE TABLE'
    END
GO

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_RootProcessId'
			AND object_id = OBJECT_ID('WorkflowProcessInstance')
		)
BEGIN
	CREATE INDEX IX_RootProcessId ON WorkflowProcessInstance (RootProcessId)
END
GO

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME] = N'WorkflowProcessInstance' 
AND [COLUMN_NAME] = N'StartingTransition')
BEGIN
	ALTER TABLE WorkflowProcessInstance ADD StartingTransition nvarchar (max) NULL
	PRINT 'ADD WorkflowProcessInstance.StartingTransition type nvarchar(max) NULL'
END
GO

UPDATE WorkflowProcessInstance SET StartingTransition = (SELECT StartingTransition FROM WorkflowProcessScheme wps WHERE wps.Id = SchemeId)
GO