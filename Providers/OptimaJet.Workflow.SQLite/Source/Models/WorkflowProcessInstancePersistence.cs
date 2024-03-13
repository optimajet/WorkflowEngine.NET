using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    public class WorkflowProcessInstancePersistence : DbObject<ProcessInstancePersistenceEntity>
    {
        public WorkflowProcessInstancePersistence(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstancePersistence", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ProcessId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ParameterName)},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Value)}
            });
        }

        public async Task<ProcessInstancePersistenceEntity[]> SelectByProcessIdAsync(SqliteConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId)} = @processid";
            var p = new SqliteParameter("processid", DbType.String) { Value = ToDbValue(processId, DbType.Guid) };
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }

        public async Task<ProcessInstancePersistenceEntity> SelectByNameAsync(SqliteConnection connection, Guid processId,  string parameterName)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId)} = @processid " + 
                                $"AND {nameof(ProcessInstancePersistenceEntity.ParameterName)} = @parameterName";

            var parameters = new List<SqliteParameter>
            {
                new SqliteParameter("processid", DbType.String) {Value = ToDbValue(processId, DbType.Guid)},
                new SqliteParameter("parameterName", DbType.String) {Value = parameterName}
            };
            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<int> DeleteByProcessIdAsync(SqliteConnection connection, Guid processId, SqliteTransaction transaction = null)
        {
            var p = new SqliteParameter("processid", DbType.String) { Value = ToDbValue(processId, DbType.Guid) };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId)} = @processid",
                    transaction,
                    p)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByNameAsync(SqliteConnection connection, Guid processId, string parameterName, SqliteTransaction transaction = null)
        {
            var parameters = new List<SqliteParameter>
            {
                new SqliteParameter("processid", DbType.String) {Value = ToDbValue(processId, DbType.Guid)},
                new SqliteParameter("parameterName", DbType.String) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId)} = @processid " +
                    $"AND {nameof(ProcessInstancePersistenceEntity.ParameterName)} = @parameterName",
                    transaction,
                    parameters.ToArray())
                .ConfigureAwait(false);
        }
    }
}
