using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.DbPersistence;

namespace OptimaJet.Workflow.MSSQL.Models;

public class WorkflowForm : DbObject<WorkflowFormEntity>
{
    public WorkflowForm(string schemaName, int commandTimeout) : base(schemaName, nameof(WorkflowForm), commandTimeout)
    {
        DBColumns.AddRange([
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Name), Size = 512 },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Version), Type = SqlDbType.Int },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.CreationDate), Type = SqlDbType.DateTime },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.UpdatedDate), Type = SqlDbType.DateTime },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Definition), Size = -1 },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Lock), Type = SqlDbType.Int }
        ]);
    }

    public async Task<List<string>> GetFormNamesAsync(SqlConnection connection)
    {
        string selectText = $"SELECT DISTINCT [Name] FROM {ObjectName}";

        WorkflowFormEntity[] formNames = await SelectAsync(connection, selectText).ConfigureAwait(false);

        return formNames.Select(f => f.Name).ToList();
    }

    public async Task<WorkflowFormEntity> GetFormAsync(SqlConnection connection, string name, int? version = null)
    {
        string selectText;

        if (version is null)
        {
            selectText = $"""
                          SELECT TOP 1 *
                          FROM {ObjectName}
                          WHERE [Name] = @Name
                          ORDER BY [Version] DESC
                          """;
        }
        else
        {
            selectText = $"""
                          SELECT *
                          FROM {ObjectName}
                          WHERE [Name] = @Name
                            AND [Version] = @Version
                          """;
        }

        var parameters = new List<SqlParameter> { new("Name", DBColumns.Find(c => c.Name == "Name").Type) { Value = name } };
        if (version is not null)
        {
            parameters.Add(new("Version", DBColumns.Find(c => c.Name == "Version").Type) { Value = version });
        }

        WorkflowFormEntity[] workflowForms = await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);

        return workflowForms.FirstOrDefault();
    }

    public async Task<List<int>> GetFormVersionsAsync(SqlConnection connection, string name)
    {
        string selectText = $"SELECT [Version] FROM {ObjectName} WHERE [Name] = @Name";

        SqlParameter[] parameters = [new("Name", DBColumns.Find(c => c.Name == "Name").Type) { Value = name }];

        WorkflowFormEntity[] workflowForms = await SelectAsync(connection, selectText, parameters).ConfigureAwait(false);

        return workflowForms.Select(f => f.Version).ToList();
    }

    public async Task<WorkflowFormEntity> CreateNewFormVersionAsync(SqlConnection connection, DateTime creationDate, string name,
        string defaultDefinition, int? version = null)
    {
        string commandText;
        string objectName = ObjectName;

        if (version is null)
        {
            commandText = $"""
                           INSERT INTO {objectName} ([Id], [Name], [Version], [CreationDate], [UpdatedDate], [Definition], [Lock])
                           OUTPUT INSERTED.*
                           SELECT NEWID(),
                                  @Name,
                                  MAX([Version]) + 1,
                                  @Date,
                                  @Date,
                                  (SELECT TOP 1 [Definition] FROM {objectName} WHERE [Name] = @Name ORDER BY [Version] DESC),
                                  0
                           FROM {objectName}
                           WHERE [Name] = @Name
                           HAVING COUNT(*) > 0

                           UNION ALL

                           SELECT NEWID(),
                                  @Name,
                                  0,
                                  @Date,
                                  @Date,
                                  @Definition,
                                  0
                           WHERE NOT EXISTS (SELECT 1 FROM {objectName} WHERE [Name] = @Name);
                           """;
        }
        else
        {
            commandText = $"""
                           INSERT INTO {objectName} ([Id], [Name], [Version], [CreationDate], [UpdatedDate], [Definition], [Lock])
                           OUTPUT INSERTED.*
                           SELECT NEWID(),
                                  @Name,
                                  (SELECT MAX([Version]) + 1 FROM {objectName} WHERE [Name] = @Name),
                                  @Date,
                                  @Date,
                                  [Definition],
                                  0 as [Lock]
                           FROM {objectName}
                           WHERE [Name] = @Name AND [Version] = @Version
                           """;
        }

        var parameters = new List<SqlParameter>
        {
            new("Name", DBColumns.Find(c => c.Name == "Name").Type) { Value = name },
            new("Date", DBColumns.Find(c => c.Name == "CreationDate").Type) { Value = creationDate }
        };
        if (version is not null)
        {
            parameters.Add(new SqlParameter("Version", DBColumns.Find(c => c.Name == "Version").Type) { Value = version });
        }
        else
        {
            parameters.Add(new("Definition",DBColumns.Find(c => c.Name == "Definition").Type) { Value = defaultDefinition });
        }

        WorkflowFormEntity[] forms = await SelectWithTransactionAsync(connection, commandText, parameters.ToArray()).ConfigureAwait(false);
        return forms.FirstOrDefault();
    }

    public async Task<WorkflowFormEntity> CreateNewFormIfNotExistsAsync(SqlConnection connection, DateTime creationDate, string name, string defaultDefinition)
    {
        string objectName = ObjectName;
        string commandText = $"""
                              MERGE INTO {objectName} WITH (HOLDLOCK) AS target
                              USING (SELECT @Name AS [Name], @Date AS [Date]) AS source
                              ON target.[Name] = source.[Name]
                              WHEN NOT MATCHED THEN
                                  INSERT ([Id], [Name], [Version], [CreationDate], [UpdatedDate], [Definition], [Lock])
                                  VALUES (NEWID(), source.[Name], 0, source.[Date], source.[Date], @Definition, 0);
                              SELECT TOP 1 *
                              FROM {objectName}
                              WHERE [Name] = @Name
                              ORDER BY [Version] DESC
                              """;

        var parameters = new List<SqlParameter>
        {
            new("Name", DBColumns.Find(c => c.Name == "Name").Type) { Value = name },
            new("Date", DBColumns.Find(c => c.Name == "CreationDate").Type) { Value = creationDate },
            new("Definition",DBColumns.Find(c => c.Name == "Definition").Type) { Value = defaultDefinition }
        };
        WorkflowFormEntity[] forms = await SelectWithTransactionAsync(connection, commandText, parameters.ToArray()).ConfigureAwait(false);
        return forms.FirstOrDefault();
    }

    public async Task<int> UpdateFormAsync(SqlConnection connection, string name, int version, long oldLock, long newLock,
        string definition, DateTime updatedDate)
    {
        string commandText = $"""
                              UPDATE {ObjectName}
                              SET [Definition]  = @Definition,
                                  [Lock]        = @NewLock,
                                  [UpdatedDate] = @Date
                              WHERE [Name] = @Name
                                AND [Version] = @Version
                                AND [Lock] = @OldLock
                              """;
        SqlParameter[] parameters =
        [
            new("Definition", DBColumns.Find(c => c.Name == "Definition").Type) { Value = definition },
            new("OldLock", DBColumns.Find(c => c.Name == "Lock").Type) { Value = oldLock },
            new("NewLock", DBColumns.Find(c => c.Name == "Lock").Type) { Value = newLock },
            new("Name", DBColumns.Find(c => c.Name == "Name").Type) { Value = name },
            new("Version", DBColumns.Find(c => c.Name == "Version").Type) { Value = version },
            new("Date", DBColumns.Find(c => c.Name == "UpdatedDate").Type) { Value = updatedDate }
        ];

        return await ExecuteCommandNonQueryAsync(connection, commandText, parameters).ConfigureAwait(false);
    }

    public async Task DeleteFormVersionAsync(SqlConnection connection, string name, int version)
    {
        string commandText = $"DELETE FROM {ObjectName} WHERE [Name] = @Name AND [Version] = @Version";
        SqlParameter[] parameters =
        [
            new("Name", DBColumns.Find(c => c.Name == "Name").Type) { Value = name },
            new("Version", DBColumns.Find(c => c.Name == "Version").Type) { Value = version }
        ];
        await ExecuteCommandNonQueryAsync(connection, commandText, parameters).ConfigureAwait(false);
    }

    public async Task DeleteFormAsync(SqlConnection connection, string name)
    {
        await DeleteByAsync(connection, f => f.Name, name);
    }
}
