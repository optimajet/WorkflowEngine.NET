using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.Oracle.Models;

public class WorkflowForm : DbObject<WorkflowFormEntity>
{
    public WorkflowForm(string schemaName, int commandTimeout) : base(schemaName, nameof(WorkflowForm), commandTimeout)
    {
        DBColumns.AddRange([
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Id), IsKey = true, Type = OracleDbType.Raw },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Name) },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Version), Type = OracleDbType.Int32 },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.CreationDate), Type = OracleDbType.TimeStamp },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.UpdatedDate), Type = OracleDbType.TimeStamp },
            new ColumnInfo { Name = nameof(WorkflowFormEntity.Definition) },
            new ColumnInfo { Name = "LOCKFLAG", Type = OracleDbType.Int32 }
        ]);
    }

    public async Task<List<string>> GetFormNamesAsync(OracleConnection connection)
    {
        string commandText = $"SELECT DISTINCT {nameof(WorkflowFormEntity.Name)} FROM {ObjectName}";
        WorkflowFormEntity[] formNames = await SelectAsync(connection, commandText).ConfigureAwait(false);
        return formNames.Select(f => f.Name).ToList();
    }

    public async Task<WorkflowFormEntity> GetFormAsync(OracleConnection connection, string name, int? version = null)
    {
        string commandText = version is null
            ? $"SELECT * FROM {ObjectName} WHERE {nameof(WorkflowFormEntity.Name)} = :name ORDER BY {nameof(WorkflowFormEntity.Version)} DESC FETCH NEXT 1 ROWS ONLY"
            : $"SELECT * FROM {ObjectName} WHERE {nameof(WorkflowFormEntity.Name)} = :name AND {nameof(WorkflowFormEntity.Version)} = :version";

        var parameters = new List<OracleParameter> { CreateParameter(nameof(WorkflowFormEntity.Name), "name", name) };

        if (version is not null)
        {
            parameters.Add(CreateParameter(nameof(WorkflowFormEntity.Version), "version", version.Value));
        }

        WorkflowFormEntity[] forms = await SelectAsync(connection, commandText, parameters.ToArray()).ConfigureAwait(false);
        return forms.FirstOrDefault();
    }

    public async Task<List<int>> GetFormVersionsAsync(OracleConnection connection, string name)
    {
        string commandText =
            $"SELECT {nameof(WorkflowFormEntity.Version)} FROM {ObjectName} WHERE {nameof(WorkflowFormEntity.Name)} = :name";
        OracleParameter parameter = CreateParameter(nameof(WorkflowFormEntity.Name), "name", name);

        WorkflowFormEntity[] forms = await SelectAsync(connection, commandText, parameter).ConfigureAwait(false);
        return forms.Select(f => f.Version).ToList();
    }

    public async Task<WorkflowFormEntity> CreateNewFormVersionAsync(OracleConnection connection, DateTime creationDate, string name,
        string defaultDefinition, int? version = null)
    {
        var id = Guid.NewGuid();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync().ConfigureAwait(false);
        }

        await using OracleTransaction transaction = connection.BeginTransaction();

        string selectLastDefinitionQuery = $"""
                                            SELECT {nameof(WorkflowFormEntity.Definition)}, {nameof(WorkflowFormEntity.Version)}
                                            FROM {ObjectName}
                                            WHERE ROWID IN (
                                                SELECT ROWID FROM {ObjectName}
                                                WHERE {nameof(WorkflowFormEntity.Name)} = :name
                                                ORDER BY {nameof(WorkflowFormEntity.Version)} DESC
                                                FETCH FIRST 1 ROWS ONLY
                                            )
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
                                                    SELECT {nameof(WorkflowFormEntity.Definition)}
                                                    FROM {ObjectName}
                                                    WHERE {nameof(WorkflowFormEntity.Name)} = :name AND {nameof(WorkflowFormEntity.Version)} = :version
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
                              INSERT INTO {ObjectName} (
                                  {nameof(WorkflowFormEntity.Id)}, {nameof(WorkflowFormEntity.Name)}, {nameof(WorkflowFormEntity.Version)},
                                  {nameof(WorkflowFormEntity.CreationDate)}, {nameof(WorkflowFormEntity.UpdatedDate)},
                                  {nameof(WorkflowFormEntity.Definition)}, LOCKFLAG
                              ) VALUES (
                                  :id, :name, :version, :creationDate, :creationDate, :definition, 0
                              )
                              """;

        OracleParameter[] parameters =
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

    public async Task<WorkflowFormEntity> CreateNewFormIfNotExistsAsync(OracleConnection connection, DateTime creationDate, string name,
        string defaultDefinition)
    {
        string command = $"""
                          DECLARE
                           v_id RAW(16) := :id;
                           v_name NVARCHAR2(512) := :name;
                           v_date TIMESTAMP := :date;
                           v_definition NVARCHAR2(512) := :definition;
                          BEGIN
                           MERGE INTO {ObjectName} target
                           USING (
                               SELECT v_id AS ID, v_name AS NAME, v_date AS CREATIONDATE, v_definition AS EMPTY_DEFINITION FROM DUAL
                           ) source
                           ON (target.{nameof(WorkflowFormEntity.Name)} = source.NAME)
                           WHEN NOT MATCHED THEN
                               INSERT ({nameof(WorkflowFormEntity.Id)}, {nameof(WorkflowFormEntity.Name)}, {nameof(WorkflowFormEntity.Version)},
                                       {nameof(WorkflowFormEntity.CreationDate)}, {nameof(WorkflowFormEntity.UpdatedDate)},
                                       {nameof(WorkflowFormEntity.Definition)}, LOCKFLAG)
                               VALUES (source.ID, source.NAME, 0, source.CREATIONDATE, source.CREATIONDATE, source.EMPTY_DEFINITION, 0);
                          END;
                          """;

        OracleParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Id), "id", Guid.NewGuid().ToByteArray()),
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.CreationDate), "date", creationDate),
            CreateParameter(nameof(WorkflowFormEntity.Definition), "definition", defaultDefinition)
        ];

        await ExecuteCommandNonQueryAsync(connection, command, parameters).ConfigureAwait(false);

        string selectCommand = $"""
                                SELECT * FROM {ObjectName}
                                WHERE {nameof(WorkflowFormEntity.Name)} = :name
                                ORDER BY {nameof(WorkflowFormEntity.Version)} DESC
                                FETCH FIRST 1 ROWS ONLY
                                """;

        WorkflowFormEntity[] result = await SelectAsync(connection, selectCommand,
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name)).ConfigureAwait(false);
        return result.FirstOrDefault();
    }

    public async Task<int> UpdateFormAsync(OracleConnection connection, string name, int version, long oldLock, long newLock,
        string definition, DateTime updatedDate)
    {
        string command = $"""
                          UPDATE {ObjectName} SET
                              {nameof(WorkflowFormEntity.Definition)} = :pDefText,
                              LOCKFLAG = :pNewLock,
                              {nameof(WorkflowFormEntity.UpdatedDate)} = :pDate
                          WHERE {nameof(WorkflowFormEntity.Name)} = :pName
                            AND {nameof(WorkflowFormEntity.Version)} = :pVersion
                            AND LOCKFLAG = :pOldLock
                          """;

        OracleParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Definition), "pDefText", definition),
            CreateParameter("LOCKFLAG", "pNewLock", newLock),
            CreateParameter("LOCKFLAG", "pOldLock", oldLock),
            CreateParameter(nameof(WorkflowFormEntity.Name), "pName", name),
            CreateParameter(nameof(WorkflowFormEntity.Version), "pVersion", version),
            CreateParameter(nameof(WorkflowFormEntity.UpdatedDate), "pDate", updatedDate)
        ];

        return await ExecuteCommandNonQueryAsync(connection, command, parameters).ConfigureAwait(false);
    }

    public async Task DeleteFormVersionAsync(OracleConnection connection, string name, int version)
    {
        string command =
            $"DELETE FROM {ObjectName} WHERE {nameof(WorkflowFormEntity.Name)} = :name AND {nameof(WorkflowFormEntity.Version)} = :version";

        OracleParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name),
            CreateParameter(nameof(WorkflowFormEntity.Version), "version", version)
        ];

        await ExecuteCommandNonQueryAsync(connection, command, parameters).ConfigureAwait(false);
    }

    public async Task DeleteFormAsync(OracleConnection connection, string name)
    {
        string command = $"DELETE FROM {ObjectName} WHERE {nameof(WorkflowFormEntity.Name)} = :name";

        OracleParameter[] parameters =
        [
            CreateParameter(nameof(WorkflowFormEntity.Name), "name", name)
        ];

        await ExecuteCommandNonQueryAsync(connection, command, parameters).ConfigureAwait(false);
    }

    private OracleParameter CreateParameter(string columnName, string parameterName, object value)
    {
        OracleDbType type = DBColumns.Find(c => c.Name == columnName).Type;
        return new OracleParameter(parameterName, type) { Value = value };
    }
}
