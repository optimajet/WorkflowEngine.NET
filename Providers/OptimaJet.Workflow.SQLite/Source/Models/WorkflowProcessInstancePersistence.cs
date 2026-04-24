using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;

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

       public async Task UpsertAsync(SqliteConnection connection, ProcessInstancePersistenceEntity entity,
            bool preferUpdateFirst, SqliteTransaction transaction)
        {
            // Optimistic approach: choose UPDATE or INSERT first, with fallback
            if (preferUpdateFirst)
            {
                // Try UPDATE first (optimistic: parameter exists in DB)
                int rowsAffected = await UpdateByNameAsync(connection, entity, transaction).ConfigureAwait(false);

                // Fallback: if UPDATE didn't affect any rows (someone deleted it), do INSERT
                if (rowsAffected == 0)
                {
                    await InsertAsync(connection, entity, transaction).ConfigureAwait(false);
                }
            }
            else
            {
                // Try INSERT first (optimistic: parameter doesn't exist in DB)
                try
                {
                    await InsertAsync(connection, entity, transaction).ConfigureAwait(false);
                }
                catch (PersistenceProviderQueryException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
                {
                    // Fallback: if INSERT failed due to the duplicate key (already exists), do UPDATE
                    await UpdateByNameAsync(connection, entity, transaction).ConfigureAwait(false);
                }
            }
        }
       
        private async Task<int> UpdateByNameAsync(SqliteConnection connection, ProcessInstancePersistenceEntity entity,
            SqliteTransaction transaction = null)
        {
            var parameters = new List<SqliteParameter>
            {
                new("processid", DbType.String) {Value = ToDbValue(entity.ProcessId, DbType.Guid)},
                new("parameterName", DbType.String) {Value = entity.ParameterName},
                new("value", DbType.String) {Value = entity.Value}
            };

            string commandText = $"UPDATE {ObjectName} " +
                                 $"SET {nameof(ProcessInstancePersistenceEntity.Value)} = @value " +
                                 $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId)} = @processid " +
                                 $"AND {nameof(ProcessInstancePersistenceEntity.ParameterName)} = @parameterName";

            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters.ToArray())
                .ConfigureAwait(false);
        }
    }
}
