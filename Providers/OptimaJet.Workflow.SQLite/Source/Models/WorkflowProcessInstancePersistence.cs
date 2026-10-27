using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    /// <summary>
    /// Database object for workflow process instance persistence parameter operations.
    /// </summary>
    public class WorkflowProcessInstancePersistence : DbObject<ProcessInstancePersistenceEntity>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WorkflowProcessInstancePersistence"/>.
        /// </summary>
        /// <param name="schemaName">The database schema name.</param>
        /// <param name="commandTimeout">The command timeout in seconds.</param>
        public WorkflowProcessInstancePersistence(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstancePersistence", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ProcessId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ParameterName)},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Value)},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.TenantId)}
            });
        }

        /// <summary>
        /// Selects all persistence parameter entities for the specified process.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="processId">The process identifier.</param>
        /// <returns>An array of persistence parameter entities.</returns>
        public async Task<ProcessInstancePersistenceEntity[]> SelectByProcessIdAsync(SqliteConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId)} = @processid";
            var p = new SqliteParameter("processid", DbType.String) { Value = ToDbValue(processId, DbType.Guid) };
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects a single persistence parameter entity by process identifier and parameter name.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <returns>The matching entity, or <c>null</c> if not found.</returns>
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

        /// <summary>
        /// Deletes all persistence parameters for the specified process.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="transaction">An optional transaction.</param>
        /// <returns>The number of rows affected.</returns>
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

        /// <summary>
        /// Deletes a single persistence parameter by process identifier and parameter name.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="transaction">An optional transaction.</param>
        /// <returns>The number of rows affected.</returns>
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

        /// <summary>
        /// Inserts or updates a persistence parameter, choosing the operation order based on <paramref name="preferUpdateFirst"/>.
        /// When <paramref name="preferUpdateFirst"/> is <c>true</c>, attempts UPDATE first with INSERT fallback.
        /// Otherwise, attempts INSERT first with UPDATE fallback on duplicate key.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="entity">The persistence parameter entity to upsert.</param>
        /// <param name="preferUpdateFirst">Whether to attempt UPDATE before INSERT.</param>
        /// <param name="transaction">The transaction to execute within.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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
                catch (PersistenceProviderQueryException ex) when (ex.IsDuplicateKeyException())
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
