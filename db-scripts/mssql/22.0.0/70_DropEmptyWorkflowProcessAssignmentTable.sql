IF OBJECT_ID(N'[dbo].[WorkflowProcessAssignment]', N'U') IS NOT NULL
BEGIN
    DECLARE @WorkflowProcessAssignmentHasRows BIT = 0;

    EXEC sp_executesql
        N'IF EXISTS (SELECT 1 FROM [dbo].[WorkflowProcessAssignment]) SET @HasRows = 1',
        N'@HasRows BIT OUTPUT',
        @HasRows = @WorkflowProcessAssignmentHasRows OUTPUT;

    IF @WorkflowProcessAssignmentHasRows = 0
    BEGIN
        EXEC(N'DROP TABLE [dbo].[WorkflowProcessAssignment]');
    END
END

-- DbScriptPublisher: mark migration as applied
IF OBJECT_ID(N'[dbo].[VersionInfo]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM [dbo].[VersionInfo]
        WHERE [Version] = 70
    )
    BEGIN
        INSERT INTO [dbo].[VersionInfo] (
            [Version],
            [AppliedOn],
            [Description]
        )
        VALUES (
            70,
            GETDATE(),
            N'Migration70DropEmptyWorkflowProcessAssignmentTable'
        );
    END;
END;
