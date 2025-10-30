using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.MySQL.Models;

public class WorkflowForm : DbObject<WorkflowFormEntity>
{
    public WorkflowForm(int commandTimeout) : base("workflowform", commandTimeout)
    {
        DBColumns.AddRange([
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Id), IsKey = true, Type = MySqlDbType.Binary },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Name) },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Version), Type = MySqlDbType.Int32 },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.CreationDate), Type = MySqlDbType.DateTime },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.UpdatedDate), Type = MySqlDbType.DateTime },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Definition) },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Lock), Type = MySqlDbType.Int32 }
        ]);
    }

    public async Task<List<string>> GetFormNamesAsync(MySqlConnection connection)
    {
        string commandText = $"SELECT DISTINCT `{nameof(WorkflowFormEntity.Name)}` FROM {DbTableName}";
        WorkflowFormEntity[] formNames = await SelectAsync(connection, commandText).ConfigureAwait(false);
        return formNames.Select(f => f.Name).ToList();
    }

    public async Task<WorkflowFormEntity> GetFormAsync(MySqlConnection connection, string name, int? version = null)
    {
        string commandText = version is null
            ? $"SELECT * FROM {DbTableName} WHERE `{nameof(WorkflowFormEntity.Name)}` = @name ORDER BY `{nameof(WorkflowFormEntity.Version)}` DESC LIMIT 1"
            : $"SELECT * FROM {DbTableName} WHERE `{nameof(WorkflowFormEntity.Name)}` = @name AND `{nameof(WorkflowFormEntity.Version)}` = @version";

        var parameters = new List<MySqlParameter> { CreateParameter(nameof(WorkflowFormEntity.Name), "name", name) };

        if (version is not null)
        {
            parameters.Add(CreateParameter(nameof(WorkflowFormEntity.Version), "version", version));
        }

        WorkflowFormEntity[] workflowForms = await SelectAsync(connection, commandText, parameters.ToArray()).ConfigureAwait(false);
        return workflowForms.FirstOrDefault();
    }

    public async Task<List<int>> GetFormVersionsAsync(MySqlConnection connection, string name)
    {
        string commandText =
            $"SELECT `{nameof(WorkflowFormEntity.Version)}` FROM {DbTableName} WHERE `{nameof(WorkflowFormEntity.Name)}` = @name";

        MySqlParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name)
        ];

        WorkflowFormEntity[] workflowForms = await SelectAsync(connection, commandText, parameters).ConfigureAwait(false);
        return workflowForms.Select(f => f.Version).ToList();
    }

    public async Task<WorkflowFormEntity> CreateNewFormVersionAsync(MySqlConnection connection, DateTime creationDate, string name,
        string defaultDefinition, int? version = null)
    {
        var id = Guid.NewGuid();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync().ConfigureAwait(false);
        }

        // ReSharper disable once UseAwaitUsing
        using MySqlTransaction transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

        string selectLastDefinitionQuery = $"""
                                            SELECT `{nameof(WorkflowFormEntity.Definition)}`, `{nameof(WorkflowFormEntity.Version)}`
                                            FROM {DbTableName}
                                            WHERE `{nameof(WorkflowFormEntity.Name)}` = @name
                                            ORDER BY `{nameof(WorkflowFormEntity.Version)}` DESC
                                            LIMIT 1
                                            FOR UPDATE
                                            """;

        WorkflowFormEntity[] formEntries = await SelectAsync(connection, selectLastDefinitionQuery, transaction,
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name)).ConfigureAwait(false);

        WorkflowFormEntity lastFormEntry = formEntries.FirstOrDefault();
        string definition = lastFormEntry?.Definition ?? defaultDefinition;
        int newVersion = lastFormEntry != null ? lastFormEntry.Version + 1 : 0;

        if (version is not null)
        {
            string selectSpecificDefinitionQuery = $"""
                                                    SELECT `{nameof(WorkflowFormEntity.Definition)}`
                                                    FROM {DbTableName}
                                                    WHERE `{nameof(WorkflowFormEntity.Name)}` = @name 
                                                    AND `{nameof(WorkflowFormEntity.Version)}` = @version
                                                    FOR UPDATE
                                                    """;
            formEntries = await SelectAsync(connection, selectSpecificDefinitionQuery, transaction,
                CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
                CreateParameter(nameof(WorkflowFormEntity.Version), "version", version.Value)).ConfigureAwait(false);

            WorkflowFormEntity specificFormEntry = formEntries.FirstOrDefault();
            if (specificFormEntry is null)
            {
                return null;
            }

            definition = specificFormEntry.Definition;
        }

        string insertQuery = $"""
                              INSERT INTO {DbTableName}
                              (
                                 `{nameof(WorkflowFormEntity.Id)}`, `{nameof(WorkflowFormEntity.Name)}`, `{nameof(WorkflowFormEntity.Version)}`,
                                 `{nameof(WorkflowFormEntity.CreationDate)}`, `{nameof(WorkflowFormEntity.UpdatedDate)}`,
                                 `{nameof(WorkflowFormEntity.Definition)}`, `{nameof(WorkflowFormEntity.Lock)}`
                              )
                              VALUES (@id, @name, @version, @creationDate, @creationDate, @definition, 0)
                              """;

        MySqlParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Id), "id", id.ToByteArray()),
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.Version), "version", newVersion),
            CreateParameter(nameof(WorkflowFormEntity.CreationDate), "creationDate", creationDate),
            CreateParameter(nameof(WorkflowFormEntity.Definition), "definition", definition)
        ];

        int inserted = await ExecuteCommandNonQueryAsync(connection, insertQuery, transaction, parameters).ConfigureAwait(false);

        if (inserted != 1)
        {
            throw new Exception($"There was an error inserting {DbTableName} to the database");
        }

        await transaction.CommitAsync().ConfigureAwait(false);

        return new WorkflowFormEntity
        {
            Id = id,
            Name = name,
            Version = newVersion,
            CreationDate = creationDate,
            UpdatedDate = creationDate,
            Definition = definition
        };
    }

    public async Task<WorkflowFormEntity> CreateNewFormIfNotExistsAsync(MySqlConnection connection, DateTime creationDate, string name,
        string defaultDefinition)
    {
        string commandText = $"""
                              INSERT INTO {DbTableName}
                                  (`{nameof(WorkflowFormEntity.Id)}`, `{nameof(WorkflowFormEntity.Name)}`, `{nameof(WorkflowFormEntity.Version)}`,
                                   `{nameof(WorkflowFormEntity.CreationDate)}`, `{nameof(WorkflowFormEntity.UpdatedDate)}`,
                                   `{nameof(WorkflowFormEntity.Definition)}`, `{nameof(WorkflowFormEntity.Lock)}`)
                              SELECT @id, @name, 0, @date, @date, @definition, 0
                              WHERE NOT EXISTS (
                                  SELECT 1 FROM {DbTableName} WHERE `{nameof(WorkflowFormEntity.Name)}` = @name
                              );

                              SELECT * FROM {DbTableName} WHERE `{nameof(WorkflowFormEntity.Name)}` = @name 
                              ORDER BY `{nameof(WorkflowFormEntity.Version)}` DESC
                              LIMIT 1;
                              """;

        MySqlParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Id), "id", Guid.NewGuid().ToByteArray()),
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.CreationDate), "date", creationDate),
            CreateParameter(nameof(WorkflowFormEntity.Definition), "definition", defaultDefinition)
        ];

        WorkflowFormEntity[] forms = await SelectAsync(connection, commandText, parameters).ConfigureAwait(false);
        return forms.FirstOrDefault();
    }

    public async Task<int> UpdateFormAsync(MySqlConnection connection, string name, int version, long oldLock, long newLock,
        string definition, DateTime updatedDate)
    {
        string commandText = $"""
                              UPDATE {DbTableName}
                              SET `{nameof(WorkflowFormEntity.Definition)}` = @definition,
                                  `{nameof(WorkflowFormEntity.Lock)}` = @newLock,
                                  `{nameof(WorkflowFormEntity.UpdatedDate)}` = @date
                              WHERE `{nameof(WorkflowFormEntity.Name)}` = @name
                                AND `{nameof(WorkflowFormEntity.Version)}` = @version
                                AND `{nameof(WorkflowFormEntity.Lock)}` = @oldLock;
                              """;

        MySqlParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Definition), "definition", definition),
            CreateParameter(nameof(WorkflowFormEntity.Lock), "oldLock", oldLock),
            CreateParameter(nameof(WorkflowFormEntity.Lock), "newLock", newLock),
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.Version), "version", version),
            CreateParameter(nameof(WorkflowFormEntity.UpdatedDate), "date", updatedDate)
        ];

        return await ExecuteCommandNonQueryAsync(connection, commandText, parameters).ConfigureAwait(false);
    }

    public async Task DeleteFormVersionAsync(MySqlConnection connection, string name, int version)
    {
        string commandText =
            $"DELETE FROM {DbTableName} WHERE `{nameof(WorkflowFormEntity.Name)}` = @name AND `{nameof(WorkflowFormEntity.Version)}` = @version";

        MySqlParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.Version), "version", version)
        ];

        await ExecuteCommandNonQueryAsync(connection, commandText, parameters).ConfigureAwait(false);
    }

    public async Task DeleteFormAsync(MySqlConnection connection, string name)
    {
        await DeleteByAsync(connection, f => f.Name, name).ConfigureAwait(false);
    }

    private MySqlParameter CreateParameter(string columnName, string parameterName, object value)
    {
        MySqlDbType type = DBColumns.Find(c => c.Name == columnName).Type;
        return new MySqlParameter(parameterName, type) { Value = value };
    }
}
