/*
Company: OptimaJet
Project: WF.Sample WorkflowEngine.NET
File: CreateObjects.sql
*/


BEGIN TRANSACTION

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'StructDivision')
BEGIN
	CREATE TABLE dbo.StructDivision (
	  Id uniqueidentifier NOT NULL,
	  Name nvarchar(256) NOT NULL,
	  ParentId uniqueidentifier NULL,
	  CONSTRAINT PK_StructDivision PRIMARY KEY (Id),
	  CONSTRAINT FK_StructDivision_StructDivision FOREIGN KEY (ParentId) REFERENCES dbo.StructDivision (Id)
	)
	PRINT 'StructDivision CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[VIEWS] WHERE [TABLE_NAME] = N'vStructDivisionParents')
BEGIN
	EXEC('CREATE VIEW dbo.vStructDivisionParents
	AS
	with cteRecursive as (
	 select sd.Id FirstId, sd.ParentId ParentId, sd.Id Id
	  from  [dbo].[StructDivision] sd WHERE sd.ParentId IS NOT NULL
	 union all 
	 select r.FirstId FirstId, sdr.ParentId ParentId, sdr.Id Id
	 from [dbo].[StructDivision] sdr
	 inner join cteRecursive r ON r.ParentId = sdr.Id)

	select DISTINCT FirstId Id, ParentId ParentId FROM cteRecursive ')

	PRINT 'vStructDivisionParents CREATE VIEW'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[VIEWS] WHERE [TABLE_NAME] = N'vStructDivisionParentsAndThis')
BEGIN
	EXEC('CREATE VIEW dbo.vStructDivisionParentsAndThis
	AS
	select  Id Id, Id ParentId FROM [dbo].[StructDivision]
	UNION 
	select  Id Id, ParentId ParentId FROM [dbo].[vStructDivisionParents]')

	PRINT 'vStructDivisionParentsAndThis CREATE VIEW'
END


IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'Employee')
BEGIN
	CREATE TABLE dbo.Employee (
	  Id uniqueidentifier NOT NULL,
	  Name nvarchar(256) NOT NULL,
	  StructDivisionId uniqueidentifier NOT NULL,
	  IsHead bit NOT NULL CONSTRAINT DF_Employee_IsHead DEFAULT (0),
	  CONSTRAINT PK_Employee PRIMARY KEY (Id),
	  CONSTRAINT FK_Employee_StructDivision FOREIGN KEY (StructDivisionId) REFERENCES dbo.StructDivision (Id)
	)

	PRINT 'Employee CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[VIEWS] WHERE [TABLE_NAME] = N'vHeads')
BEGIN
	EXEC('CREATE VIEW dbo.vHeads
	AS
	select  e.Id Id, e.Name Name, eh.Id HeadId, eh.Name HeadName FROM Employee e 
		INNER JOIN [vStructDivisionParentsAndThis] vsp ON e.StructDivisionId = vsp.Id
		INNER JOIN Employee eh ON eh.StructDivisionId = vsp.ParentId AND eh.IsHead = 1')
	PRINT 'vHeads CREATE VIEW'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'Document')
BEGIN
	CREATE TABLE dbo.Document (
	  Id uniqueidentifier NOT NULL,
	  Number int IDENTITY,
	  Name nvarchar(256) NOT NULL,
	  Comment nvarchar(max) NULL,
	  AuthorId uniqueidentifier NOT NULL,
	  ManagerId uniqueidentifier NULL,
	  [Sum] money NOT NULL CONSTRAINT DF_Document_Sum DEFAULT (0),
	  [State] nvarchar(1024) NOT NULL DEFAULT ('VacationRequestCreated'),
	  StateName nvarchar(1024) ,
	  CONSTRAINT PK_Document PRIMARY KEY (Id),
	  CONSTRAINT FK_Document_Employee FOREIGN KEY (ManagerId) REFERENCES dbo.Employee (Id),
	  CONSTRAINT FK_Document_Employee1 FOREIGN KEY (AuthorId) REFERENCES dbo.Employee (Id) ON DELETE CASCADE ON UPDATE CASCADE
	)

	PRINT 'Document CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'DocumentTransitionHistory')
BEGIN
	CREATE TABLE dbo.DocumentTransitionHistory (
	  Id uniqueidentifier NOT NULL,
	  DocumentId uniqueidentifier NOT NULL,
	  EmployeeId uniqueidentifier NULL,
	  AllowedToEmployeeNames nvarchar(max) NOT NULL,
	  TransitionTime datetime NULL,
	  [Order] bigint IDENTITY,
	  TransitionTimeForSort AS (coalesce([TransitionTime],CONVERT([datetime],'9999-12-31',(20)))),
	  InitialState nvarchar(1024) NOT NULL,
	  DestinationState nvarchar(1024) NOT NULL,
	  Command nvarchar(1024) NOT NULL,
	  CONSTRAINT PK_DocumentTransitionHistory PRIMARY KEY (Id),
	  CONSTRAINT FK_DocumentTransitionHistory_Document FOREIGN KEY (DocumentId) REFERENCES dbo.Document (Id) ON DELETE CASCADE ON UPDATE CASCADE,
	  CONSTRAINT FK_DocumentTransitionHistory_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee (Id)
	)

	PRINT 'DocumentTransitionHistory CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'Roles')
BEGIN
	CREATE TABLE dbo.Roles (
	  Id uniqueidentifier NOT NULL,
	  Name nvarchar(256) NOT NULL,
	  CONSTRAINT PK_Roles PRIMARY KEY (Id)
	)

	PRINT 'Roles CREATE TABLE'
END

IF NOT EXISTS (SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = N'EmployeeRole')
BEGIN
	CREATE TABLE dbo.EmployeeRole (
	  EmployeeId uniqueidentifier NOT NULL,
	  RoleId uniqueidentifier NOT NULL,
	  CONSTRAINT PK_EmployeeRoles PRIMARY KEY (EmployeeId, RoleId),
	  CONSTRAINT FK_EmployeeRole_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee (Id) ON DELETE CASCADE ON UPDATE CASCADE,
	  CONSTRAINT FK_EmployeeRole_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id) ON DELETE CASCADE ON UPDATE CASCADE
	)

	PRINT 'EmployeeRole CREATE TABLE'
END

COMMIT TRANSACTION