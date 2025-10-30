IF NOT EXISTS (
		SELECT 1
		FROM [INFORMATION_SCHEMA].[TABLES]
		WHERE [TABLE_NAME] = N'WorkflowForm'
		)
BEGIN
    CREATE TABLE WorkflowForm (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(512) NOT NULL,
        [Version] INT NOT NULL,
        [CreationDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [Definition] NVARCHAR(MAX) NOT NULL,
        [Lock] INT NOT NULL,
        CONSTRAINT [PK_WorkflowForm] PRIMARY KEY (Id),
        CONSTRAINT [UQ_WorkflowForm_Name_Version] UNIQUE (Name, Version)
    );

    PRINT 'WorkflowForm CREATE TABLE'
END