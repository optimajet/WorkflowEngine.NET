using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessInstancePersistence : DbObject<ProcessInstancePersistenceEntity>
    {
        public WorkflowProcessInstancePersistence(int commandTimeout) : base("workflowprocessinstancepersistence", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ParameterName)},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Value), Type = MySqlDbType.LongText}
            });
        }

        public async Task<ProcessInstancePersistenceEntity[]> SelectByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {DbTableName} " + 
                                $"WHERE `{nameof(ProcessInstancePersistenceEntity.ProcessId)}` = @processid";
            var p = new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()};
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }

        public async Task<ProcessInstancePersistenceEntity> SelectByNameAsync(MySqlConnection connection, Guid processId,
            string parameterName)
        {
            string selectText = $"SELECT * FROM {DbTableName} " + 
                                $"WHERE `{nameof(ProcessInstancePersistenceEntity.ProcessId)}` = @processid " + 
                                $"AND `{nameof(ProcessInstancePersistenceEntity.ParameterName)}` = @parameterName";

            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()},
                new MySqlParameter("parameterName", MySqlDbType.VarChar) {Value = parameterName}
            };
            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId, MySqlTransaction transaction = null)
        {
            var p = new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()};

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {DbTableName} " +
                    $"WHERE `{nameof(ProcessInstancePersistenceEntity.ProcessId)}` = @processid",
                    transaction,
                    p)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByNameAsync(MySqlConnection connection, Guid processId, string parameterName,
            MySqlTransaction transaction = null)
        {
            var parameters = new List<MySqlParameter>
            {
                new("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()},
                new("parameterName", MySqlDbType.VarChar) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {DbTableName} " +
                    $"WHERE `{nameof(ProcessInstancePersistenceEntity.ProcessId)}` = @processid " +
                    $"AND `{nameof(ProcessInstancePersistenceEntity.ParameterName)}` = @parameterName",
                    transaction,
                    parameters.ToArray())
                .ConfigureAwait(false);
        }

        public async Task UpsertAsync(MySqlConnection connection, ProcessInstancePersistenceEntity entity,
            bool preferUpdateFirst, MySqlTransaction transaction)
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
                catch (MySqlException mysqlEx) when (mysqlEx.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                {
                    // Fallback: if INSERT failed due to the duplicate key, do UPDATE
                    await UpdateByNameAsync(connection, entity, transaction).ConfigureAwait(false);
                }
            }
        }
        
        private async Task<int> UpdateByNameAsync(MySqlConnection connection, ProcessInstancePersistenceEntity entity,
            MySqlTransaction transaction = null)
        {
            var parameters = new List<MySqlParameter>
            {
                new("processid", MySqlDbType.Binary) {Value = entity.ProcessId.ToByteArray()},
                new("parameterName", MySqlDbType.VarChar) {Value = entity.ParameterName},
                new("value", MySqlDbType.LongText) {Value = entity.Value}
            };

            string commandText = $"UPDATE {DbTableName} " +
                                 $"SET `{nameof(ProcessInstancePersistenceEntity.Value)}` = @value " +
                                 $"WHERE `{nameof(ProcessInstancePersistenceEntity.ProcessId)}` = @processid " +
                                 $"AND `{nameof(ProcessInstancePersistenceEntity.ParameterName)}` = @parameterName";

            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters.ToArray())
                .ConfigureAwait(false);
        }
    }
}
