using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.SQLite.Models;

public class WorkflowForm : DbObject<WorkflowFormEntity>
{
    public WorkflowForm(string schemaName, int commandTimeout) : base(schemaName, nameof(WorkflowForm), commandTimeout)
    {
        DBColumns.AddRange([
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Id), IsKey = true, Type = DbType.Guid },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Name) },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Version), Type = DbType.Int32 },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.CreationDate), Type = DbType.DateTime2 },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.UpdatedDate), Type = DbType.DateTime2 },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Definition) },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Lock), Type = DbType.Int32 }
        ]);
    }

    public async Task<List<string>> GetFormNamesAsync(SqliteConnection connection)
    {
        string query = $"SELECT DISTINCT [{nameof(WorkflowFormEntity.Name)}] FROM {ObjectName}";
        WorkflowFormEntity[] forms = await SelectAsync(connection, query).ConfigureAwait(false);
        return forms.Select(f => f.Name).ToList();
    }

    public async Task<WorkflowFormEntity> GetFormAsync(SqliteConnection connection, string name, int? version = null)
    {
        string query = version is null
            ? $"""
                 SELECT *
                 FROM {ObjectName}
                 WHERE [{nameof(WorkflowFormEntity.Name)}] = @name
                 ORDER BY [{nameof(WorkflowFormEntity.Version)}] DESC
                 LIMIT 1
               """
            : $"""
                 SELECT *
                 FROM {ObjectName}
                 WHERE [{nameof(WorkflowFormEntity.Name)}] = @name AND [{nameof(WorkflowFormEntity.Version)}] = @version
               """;

        var parameters = new List<SqliteParameter> { CreateParameter(nameof(WorkflowFormEntity.Name), "name", name) };

        if (version is not null)
        {
            parameters.Add(CreateParameter(nameof(WorkflowFormEntity.Version), "version", version));
        }

        WorkflowFormEntity[] forms = await SelectAsync(connection, query, parameters.ToArray()).ConfigureAwait(false);
        return forms.FirstOrDefault();
    }

    public async Task<List<int>> GetFormVersionsAsync(SqliteConnection connection, string name)
    {
        string query = $"SELECT [{nameof(WorkflowFormEntity.Version)}] FROM {ObjectName} WHERE [{nameof(WorkflowFormEntity.Name)}] = @name";

        SqliteParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name)
        ];

        WorkflowFormEntity[] forms = await SelectAsync(connection, query, parameters).ConfigureAwait(false);
        return forms.Select(f => f.Version).ToList();
    }

    public async Task<WorkflowFormEntity> CreateNewFormVersionAsync(SqliteConnection connection, DateTime creationDate, string name,
        string defaultDefinition, int? version = null)
    {
        var id = Guid.NewGuid();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync().ConfigureAwait(false);
        }

        await using SqliteTransaction transaction = connection.BeginTransaction();

        string selectLastDefinitionQuery = $"""
                                            SELECT [{nameof(WorkflowFormEntity.Version)}], [{nameof(WorkflowFormEntity.Definition)}]
                                            FROM {ObjectName}
                                            WHERE [{nameof(WorkflowFormEntity.Name)}] = @name
                                            ORDER BY [{nameof(WorkflowFormEntity.Version)}] DESC
                                            LIMIT 1
                                            """;

        WorkflowFormEntity[] formEntries = await SelectAsync(connection, selectLastDefinitionQuery, transaction,
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name)).ConfigureAwait(false);

        WorkflowFormEntity lastFormEntry = formEntries.FirstOrDefault();
        string definition = lastFormEntry?.Definition ?? defaultDefinition;
        int newVersion = lastFormEntry != null ? lastFormEntry.Version + 1 : 0;

        if (version is not null)
        {
            string selectSpecificDefinitionQuery = $"""
                                                    SELECT [{nameof(WorkflowFormEntity.Definition)}]
                                                    FROM {ObjectName}
                                                    WHERE [{nameof(WorkflowFormEntity.Name)}] = @name
                                                    AND [{nameof(WorkflowFormEntity.Version)}] = @version
                                                    LIMIT 1
                                                    """;

            formEntries = await SelectAsync(connection, selectSpecificDefinitionQuery, transaction,
                    CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
                    CreateParameter(nameof(WorkflowFormEntity.Version), "version", version.Value))
                .ConfigureAwait(false);
            WorkflowFormEntity specificFormEntry = formEntries.FirstOrDefault();
            if (specificFormEntry is null)
            {
                return null;
            }

            definition = specificFormEntry.Definition;
        }

        string insertQuery = $"""
                              INSERT INTO {ObjectName} ([Id], [{nameof(WorkflowFormEntity.Name)}], [{nameof(WorkflowFormEntity.Version)}],
                              [{nameof(WorkflowFormEntity.CreationDate)}], [{nameof(WorkflowFormEntity.UpdatedDate)}],
                              [{nameof(WorkflowFormEntity.Definition)}], [{nameof(WorkflowFormEntity.Lock)}])
                              VALUES (@id, @name, @version, @date, @date, @definition, 0);
                              """;
        
        SqliteParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Id), "id", ToDbValue(id, DbType.Guid)),
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.Version), "version", newVersion),
            CreateParameter(nameof(WorkflowFormEntity.CreationDate), "date", ToDbValue(creationDate, DbType.DateTime2)),
            CreateParameter(nameof(WorkflowFormEntity.Definition), "definition", definition)
        ];

        int inserted = await ExecuteCommandNonQueryAsync(connection, insertQuery, transaction, parameters).ConfigureAwait(false);

        if (inserted != 1)
        {
            throw new Exception($"There was an error inserting {ObjectName} to the database");
        }

        transaction.Commit();

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

    public async Task<WorkflowFormEntity> CreateNewFormIfNotExistsAsync(SqliteConnection connection, DateTime creationDate, string name,
        string defaultDefinition)
    {
        string query = $"""
                        INSERT INTO {ObjectName} ([{nameof(WorkflowFormEntity.Id)}], [{nameof(WorkflowFormEntity.Name)}], 
                        [{nameof(WorkflowFormEntity.Version)}], [{nameof(WorkflowFormEntity.CreationDate)}],
                        [{nameof(WorkflowFormEntity.UpdatedDate)}], [{nameof(WorkflowFormEntity.Definition)}],
                        [{nameof(WorkflowFormEntity.Lock)}])
                        SELECT @id,
                               @name,
                               0,
                               @date,
                               @date,
                               @definition,
                               0
                        WHERE NOT EXISTS (SELECT 1 FROM {ObjectName} WHERE [{nameof(WorkflowFormEntity.Name)}] = @name);

                        SELECT *
                        FROM {ObjectName}
                        WHERE [{nameof(WorkflowFormEntity.Name)}] = @name
                        ORDER BY [{nameof(WorkflowFormEntity.Version)}] DESC
                        LIMIT 1;
                        """;

        SqliteParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Id), "id", ToDbValue(Guid.NewGuid(), DbType.Guid)),
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.CreationDate), "date", ToDbValue(creationDate, DbType.DateTime2)),
            CreateParameter(nameof(WorkflowFormEntity.Definition), "definition", defaultDefinition)
        ];

        WorkflowFormEntity[] forms = await SelectAsync(connection, query, parameters).ConfigureAwait(false);
        return forms.FirstOrDefault();
    }

    public async Task<int> UpdateFormAsync(SqliteConnection connection, string name, int version, long oldLock, long newLock,
        string definition, DateTime updatedDate)
    {
        string query = $"""
                        UPDATE {ObjectName}
                        SET [{nameof(WorkflowFormEntity.Definition)}]  = @definition,
                            [{nameof(WorkflowFormEntity.Lock)}]        = @newLock,
                            [{nameof(WorkflowFormEntity.UpdatedDate)}] = @date
                        WHERE [{nameof(WorkflowFormEntity.Name)}] = @name
                          AND [{nameof(WorkflowFormEntity.Version)}] = @version
                          AND [{nameof(WorkflowFormEntity.Lock)}] = @oldLock
                        """;

        SqliteParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Definition), "definition", definition),
            CreateParameter(nameof(WorkflowFormEntity.Lock), "oldLock", oldLock),
            CreateParameter(nameof(WorkflowFormEntity.Lock), "newLock", newLock),
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.Version), "version", version),
            CreateParameter(nameof(WorkflowFormEntity.UpdatedDate), "date", ToDbValue(updatedDate, DbType.DateTime2))
        ];

        return await ExecuteCommandNonQueryAsync(connection, query, parameters).ConfigureAwait(false);
    }

    public async Task DeleteFormVersionAsync(SqliteConnection connection, string name, int version)
    {
        string query =
            $"DELETE FROM {ObjectName} WHERE [{nameof(WorkflowFormEntity.Name)}] = @name AND [{nameof(WorkflowFormEntity.Version)}] = @version";

        var parameters = new[]
        {
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.Version), "version", version)
        };

        await ExecuteCommandNonQueryAsync(connection, query, parameters).ConfigureAwait(false);
    }

    public async Task DeleteFormAsync(SqliteConnection connection, string name)
    {
        await DeleteByAsync(connection, f => f.Name, name).ConfigureAwait(false);
    }

    private SqliteParameter CreateParameter(string columnName, string parameterName, object value)
    {
        DbType type = DBColumns.Find(c => c.Name == columnName).Type;
        return new SqliteParameter(parameterName, type) { Value = value };
    }
}
