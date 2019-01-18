using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace AngularBPWorkflow.Migrations
{
    //WorkflowEngineSampleCode
    public partial class Workflow_Db_Changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
            name: "AppDocuments",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                CreationTime = table.Column<DateTime>(nullable: false),
                Description = table.Column<string>(maxLength: 65536, nullable: true),
                State = table.Column<string>(maxLength: 256, nullable: false),
                Title = table.Column<string>(maxLength: 256, nullable: false),
                Scheme = table.Column<string>(maxLength: 256, nullable: false),
                ProcessId = table.Column<Guid>(nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppDocuments", x => x.Id);
            });

            #region createPersistenceObjectsScript
            const string createPersistenceObjectsScript = @"

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessScheme'
		)
BEGIN
	CREATE TABLE WorkflowProcessScheme (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessScheme PRIMARY KEY
		,[Scheme] NTEXT NOT NULL
		,[DefiningParameters] NTEXT NOT NULL
		,[DefiningParametersHash] NVARCHAR(24) NOT NULL
		,[SchemeCode] NVARCHAR(256) NOT NULL
		,[IsObsolete] BIT DEFAULT 0 NOT NULL
		,[RootSchemeCode] NVARCHAR(256)
		,[RootSchemeId] UNIQUEIDENTIFIER
		,[AllowedActivities] NVARCHAR(max)
		,[StartingTransition] NVARCHAR(max)
		)

	CREATE INDEX IX_SchemeCode_Hash_IsObsolete ON WorkflowProcessScheme (
		SchemeCode
		,DefiningParametersHash
		,IsObsolete
		)

	PRINT 'WorkflowProcessScheme CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessInstance'
		)
BEGIN
	CREATE TABLE WorkflowProcessInstance (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessInstance PRIMARY KEY
		,[StateName] NVARCHAR(max)
		,[ActivityName] NVARCHAR(max) NOT NULL
		,[SchemeId] UNIQUEIDENTIFIER
		,[PreviousState] NVARCHAR(max)
		,[PreviousStateForDirect] NVARCHAR(max)
		,[PreviousStateForReverse] NVARCHAR(max)
		,[PreviousActivity] NVARCHAR(max)
		,[PreviousActivityForDirect] NVARCHAR(max)
		,[PreviousActivityForReverse] NVARCHAR(max)
		,[ParentProcessId] UNIQUEIDENTIFIER
		,[RootProcessId] UNIQUEIDENTIFIER NOT NULL
		,[IsDeterminingParametersChanged] BIT DEFAULT 0 NOT NULL
		)

	PRINT 'WorkflowProcessInstance CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessInstancePersistence'
		)
BEGIN
	CREATE TABLE WorkflowProcessInstancePersistence (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessInstancePersistence PRIMARY KEY NONCLUSTERED
		,[ProcessId] UNIQUEIDENTIFIER NOT NULL
		,[ParameterName] NVARCHAR(max) NOT NULL
		,[Value] NVARCHAR(max) NOT NULL
		)

	CREATE CLUSTERED INDEX IX_ProcessId_Clustered ON WorkflowProcessInstancePersistence (ProcessId)

	PRINT 'WorkflowProcessInstancePersistence CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessTransitionHistory'
		)
BEGIN
	CREATE TABLE WorkflowProcessTransitionHistory (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessTransitionHistory PRIMARY KEY NONCLUSTERED
		,[ProcessId] UNIQUEIDENTIFIER NOT NULL
		,[ExecutorIdentityId] NVARCHAR(256)
		,[ActorIdentityId] NVARCHAR(256)
		,[FromActivityName] NVARCHAR(max) NOT NULL
		,[ToActivityName] NVARCHAR(max) NOT NULL
		,[ToStateName] NVARCHAR(max)
		,[TransitionTime] DATETIME NOT NULL
		,[TransitionClassifier] NVARCHAR(max) NOT NULL
		,[IsFinalised] BIT NOT NULL
		,[FromStateName] NVARCHAR(max)
		,[TriggerName] NVARCHAR(max)
		)

	CREATE CLUSTERED INDEX IX_ProcessId_Clustered ON WorkflowProcessTransitionHistory (ProcessId)

	CREATE INDEX IX_ExecutorIdentityId ON WorkflowProcessTransitionHistory (ExecutorIdentityId)

	PRINT 'WorkflowProcessTransitionHistory CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessInstanceStatus'
		)
BEGIN
	CREATE TABLE WorkflowProcessInstanceStatus (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessInstanceStatus PRIMARY KEY
		,[Status] TINYINT NOT NULL
		,[Lock] UNIQUEIDENTIFIER NOT NULL
		)

	PRINT 'WorkflowProcessInstanceStatus CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.procedures
		WHERE name = N'spWorkflowProcessResetRunningStatus'
		)
BEGIN
	EXECUTE (
			'CREATE PROCEDURE [spWorkflowProcessResetRunningStatus]
	AS
	BEGIN
		UPDATE [WorkflowProcessInstanceStatus] SET [WorkflowProcessInstanceStatus].[Status] = 2 WHERE [WorkflowProcessInstanceStatus].[Status] = 1
	END'
			)

	PRINT 'spWorkflowProcessResetRunningStatus CREATE PROCEDURE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowScheme'
		)
BEGIN
	CREATE TABLE WorkflowScheme (
		[Code] NVARCHAR(256) NOT NULL CONSTRAINT PK_WorkflowScheme PRIMARY KEY
		,[Scheme] NVARCHAR(max) NOT NULL
		)

	PRINT 'WorkflowScheme CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.procedures
		WHERE name = N'DropWorkflowProcess'
		)
BEGIN
	EXECUTE (
			'CREATE PROCEDURE [DropWorkflowProcess]
		@id uniqueidentifier
	AS
	BEGIN
		BEGIN TRAN

		DELETE FROM dbo.WorkflowProcessInstance WHERE Id = @id
		DELETE FROM dbo.WorkflowProcessInstanceStatus WHERE Id = @id
		DELETE FROM dbo.WorkflowProcessInstancePersistence  WHERE ProcessId = @id

		COMMIT TRAN
	END'
			)

	PRINT 'DropWorkflowProcess CREATE PROCEDURE'
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.procedures
		WHERE name = N'DropWorkflowProcesses'
		)
BEGIN
	EXECUTE (
			'CREATE TYPE IdsTableType AS TABLE
	( Id uniqueidentifier );'
			)

	PRINT 'IdsTableType CREATE TYPE'

	EXECUTE (
			'CREATE PROCEDURE [DropWorkflowProcesses]
		@Ids  IdsTableType	READONLY
	AS
	BEGIN
		BEGIN TRAN

		DELETE dbo.WorkflowProcessInstance FROM dbo.WorkflowProcessInstance wpi  INNER JOIN @Ids  ids ON wpi.Id = ids.Id
		DELETE dbo.WorkflowProcessInstanceStatus FROM dbo.WorkflowProcessInstanceStatus wpi  INNER JOIN @Ids  ids ON wpi.Id = ids.Id
		DELETE dbo.WorkflowProcessInstanceStatus FROM dbo.WorkflowProcessInstancePersistence wpi  INNER JOIN @Ids  ids ON wpi.ProcessId = ids.Id


		COMMIT TRAN
	END'
			)

	PRINT 'DropWorkflowProcesses CREATE PROCEDURE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowInbox'
		)
BEGIN
	CREATE TABLE WorkflowInbox (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowInbox PRIMARY KEY NONCLUSTERED
		,[ProcessId] UNIQUEIDENTIFIER NOT NULL
		,[IdentityId] NVARCHAR(256) NOT NULL
		)

	CREATE CLUSTERED INDEX IX_IdentityId_Clustered ON WorkflowInbox (IdentityId)

	CREATE INDEX IX_ProcessId ON WorkflowInbox (ProcessId)

	PRINT 'WorkflowInbox CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.procedures
		WHERE name = N'DropWorkflowInbox'
		)
BEGIN
	EXECUTE (
			'CREATE PROCEDURE [DropWorkflowInbox]
		@processId uniqueidentifier
	AS
	BEGIN
		BEGIN TRAN
		DELETE FROM dbo.WorkflowInbox WHERE ProcessId = @processId
		COMMIT TRAN
	END'
			)

	PRINT 'DropWorkflowInbox CREATE PROCEDURE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowProcessTimer'
		)
BEGIN
	CREATE TABLE WorkflowProcessTimer (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowProcessTimer PRIMARY KEY NONCLUSTERED
		,[ProcessId] UNIQUEIDENTIFIER NOT NULL
		,[Name] NVARCHAR(max) NOT NULL
		,[NextExecutionDateTime] DATETIME NOT NULL
		,[Ignore] BIT NOT NULL
		)

	CREATE CLUSTERED INDEX IX_NextExecutionDateTime_Clustered ON WorkflowProcessTimer (NextExecutionDateTime)

	PRINT 'WorkflowProcessTimer CREATE TABLE'
END

IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowGlobalParameter'
		)
BEGIN
	CREATE TABLE WorkflowGlobalParameter (
		[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkflowGlobalParameter PRIMARY KEY NONCLUSTERED
		,[Type] NVARCHAR(306) NOT NULL
		,[Name] NVARCHAR(128) NOT NULL
		,[Value] NVARCHAR(max) NOT NULL
		)

	CREATE UNIQUE CLUSTERED INDEX IX_Type_Name_Clustered ON WorkflowGlobalParameter (
		Type
		,Name
		)

	PRINT 'WorkflowGlobalParameter CREATE TABLE'
END

";
            #endregion

            migrationBuilder.Sql(createPersistenceObjectsScript);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {   
            migrationBuilder.DropTable(name: "AppDocuments");

            #region dropPersistenceObjectsScript
            const string dropPersistenceObjectsScript = @"
            BEGIN TRANSACTION

IF EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowProcessScheme')
BEGIN
    DROP TABLE[WorkflowProcessScheme]

    PRINT 'WorkflowProcessScheme DROP TABLE'
END

IF EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowProcessInstance')
BEGIN
    DROP TABLE[WorkflowProcessInstance]

    PRINT 'WorkflowProcessInstance DROP DROP'
END

IF EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowProcessInstancePersistence')
BEGIN
    DROP TABLE[WorkflowProcessInstancePersistence]

    PRINT 'WorkflowProcessInstancePersistence DROP TABLE'
END

IF EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowProcessTransitionHistory')
BEGIN
    DROP TABLE[WorkflowProcessTransitionHistory]

    PRINT 'WorkflowProcessTransitionHistory DROP TABLE'
END

IF EXISTS(SELECT 1 FROM sys.triggers WHERE name = N'tWorkflowProcessTransitionHistoryInsert')
BEGIN
    DROP TRIGGER[tWorkflowProcessTransitionHistoryInsert]

    PRINT 'WorkflowProcessTransitionHistory DROP TRIGGER tWorkflowProcessTransitionHistoryInsert'
END

IF EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowProcessInstanceStatus')
BEGIN
    DROP TABLE[WorkflowProcessInstanceStatus]

    PRINT 'WorkflowProcessInstanceStatus DROP TABLE'
END

IF EXISTS(SELECT 1 FROM sys.procedures WHERE name = N'spWorkflowProcessResetRunningStatus')
BEGIN
    DROP PROCEDURE[spWorkflowProcessResetRunningStatus]

    PRINT 'spWorkflowProcessResetRunningStatus DROP PROCEDURE'
END


IF EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowRuntime')
BEGIN
    DROP TABLE[WorkflowRuntime]

    PRINT 'WorkflowRuntime DROP TABLE'
END

IF EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowScheme')
BEGIN
    DROP TABLE[WorkflowScheme]

    PRINT 'WorkflowScheme DROP TABLE'
END

IF EXISTS(SELECT 1 FROM sys.procedures WHERE name = N'DropWorkflowProcess')
BEGIN
    DROP PROCEDURE[DropWorkflowProcess]

    PRINT 'DropWorkflowProcess DROP PROCEDURE'
END

IF EXISTS(SELECT 1 FROM sys.procedures WHERE name = N'DropWorkflowProcesses')
BEGIN
    DROP PROCEDURE[DropWorkflowProcesses]

    PRINT 'DropWorkflowProcesses DROP PROCEDURE'


    DROP TYPE IdsTableType
    PRINT 'IdsTableType DROP TYPE'
END

IF EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowInbox')
BEGIN
    DROP TABLE[WorkflowInbox]

    PRINT 'WorkflowInbox DROP TABLE'
END

IF EXISTS(SELECT 1 FROM sys.procedures WHERE name = N'DropWorkflowInbox')
BEGIN
    DROP PROCEDURE[DropWorkflowInbox]

    PRINT 'DropWorkflowInbox DROP PROCEDURE'
END

IF  EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowProcessTimer')
BEGIN
    DROP TABLE[WorkflowProcessTimer]

    PRINT 'WorkflowProcessTimer DROP TABLE'
END

IF  EXISTS(SELECT 1 FROM[INFORMATION_SCHEMA].[TABLES] WHERE[TABLE_NAME] = N'WorkflowGlobalParameter')
BEGIN
    DROP TABLE WorkflowGlobalParameter

    PRINT 'WorkflowGlobalParameter DROP TABLE'
END

COMMIT TRANSACTION";
            #endregion

            migrationBuilder.Sql(dropPersistenceObjectsScript);

        }
    }
}
