using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessInstancePersistence : DbObject<ProcessInstancePersistenceEntity>
    {
        public WorkflowProcessInstancePersistence(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstancePersistence", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ProcessId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ParameterName)},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Value), Type = NpgsqlDbType.Text}
            });
        }

        public async Task<ProcessInstancePersistenceEntity[]> SelectByProcessIdAsync(NpgsqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid";
            var p = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }

        public async Task<ProcessInstancePersistenceEntity> SelectByNameAsync(NpgsqlConnection connection, Guid processId,  string parameterName)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid " + 
                                $"AND \"{nameof(ProcessInstancePersistenceEntity.ParameterName)}\" = @parameterName";

            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId}, 
                new NpgsqlParameter("parameterName", NpgsqlDbType.Text) {Value = parameterName}
            };
            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction = null)
        {
            var p = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid",
                    transaction,
                    p)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByNameAsync(NpgsqlConnection connection, Guid processId, string parameterName, NpgsqlTransaction transaction = null)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId},
                new NpgsqlParameter("parameterName", NpgsqlDbType.Text) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid " +
                    $"AND \"{nameof(ProcessInstancePersistenceEntity.ParameterName)}\" = @parameterName",
                    transaction,
                    parameters.ToArray())
                .ConfigureAwait(false);
        }

        public async Task UpsertAsync(NpgsqlConnection connection, ProcessInstancePersistenceEntity entity,
            bool preferUpdateFirst, NpgsqlTransaction transaction)
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
                // Use savepoint to handle transaction abort on constraint violation (PostgreSQL specific)
                await transaction.SaveAsync("insert_param").ConfigureAwait(false);
                try
                {
                    await InsertAsync(connection, entity, transaction).ConfigureAwait(false);
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "23505")
                {
                    // Rollback to savepoint to continue transaction after the error
                    await transaction.RollbackAsync("insert_param").ConfigureAwait(false);

                    // Fallback: if INSERT failed due to the duplicate key, do UPDATE
                    await UpdateByNameAsync(connection, entity, transaction).ConfigureAwait(false);
                }
            }
        }
        
        private async Task<int> UpdateByNameAsync(NpgsqlConnection connection, ProcessInstancePersistenceEntity entity, NpgsqlTransaction transaction = null)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new("processid", NpgsqlDbType.Uuid) {Value = entity.ProcessId},
                new("parameterName", NpgsqlDbType.Text) {Value = entity.ParameterName},
                new("value", NpgsqlDbType.Text) {Value = (object)entity.Value ?? DBNull.Value}
            };

            string commandText = $@"UPDATE {ObjectName} " +
                                 $"SET \"{nameof(ProcessInstancePersistenceEntity.Value)}\" = @value " +
                                 $"WHERE \"{nameof(ProcessInstancePersistenceEntity.ProcessId)}\" = @processid " +
                                 $"AND \"{nameof(ProcessInstancePersistenceEntity.ParameterName)}\" = @parameterName";

            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters.ToArray())
                .ConfigureAwait(false);
        }
    }
}
