using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessInstancePersistence : DbObject<ProcessInstancePersistenceEntity>
    {
        public WorkflowProcessInstancePersistence(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstanceP", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Id), IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ProcessId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ParameterName)},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Value), Type = OracleDbType.NClob}
            });
        }

        public async Task<ProcessInstancePersistenceEntity[]> SelectByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId)} = :processid";
            
            return await SelectAsync(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input)).ConfigureAwait(false);
        }

        public async Task<ProcessInstancePersistenceEntity> SelectByNameAsync(OracleConnection connection,
            Guid processId, string parameterName)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId)} = :processid " + 
                                $"AND {nameof(ProcessInstancePersistenceEntity.ParameterName)} = :parameterName";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input),
                new OracleParameter("parameterName", OracleDbType.NVarchar2, parameterName, ParameterDirection.Input)
            };
            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId, OracleTransaction transaction)
        {
            return await ExecuteCommandNonQueryAsync(
                    connection,
                    $"DELETE FROM {ObjectName} " + 
                    $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId).ToUpperInvariant()} = :processid",
                    transaction,
                    new OracleParameter(
                        "processid",
                        OracleDbType.Raw,
                        processId.ToByteArray(),
                        ParameterDirection.Input))
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByNameAsync(OracleConnection connection, Guid processId, string parameterName)
        {
            return await DeleteByNameAsync(connection, processId, parameterName, null).ConfigureAwait(false);
        }

        public async Task<int> DeleteByNameAsync(OracleConnection connection, Guid processId, string parameterName, 
            OracleTransaction transaction)
        {
            var parameters = new List<OracleParameter>
            {
                new("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input),
                new("parameterName", OracleDbType.NVarchar2, parameterName, ParameterDirection.Input)
            };

            return await ExecuteCommandNonQueryAsync(
                connection,
                $"DELETE FROM {ObjectName} " + 
                $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId).ToUpperInvariant()} = :processid " +
                $"AND {nameof(ProcessInstancePersistenceEntity.ParameterName).ToUpperInvariant()} = :parameterName",
                transaction,
                parameters.ToArray()).ConfigureAwait(false);
        }

        public async Task UpsertAsync(OracleConnection connection, ProcessInstancePersistenceEntity entity,
            bool preferUpdateFirst, OracleTransaction transaction)
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
                catch (OracleException oraEx) when (oraEx.Number == 1) // ORA-00001: unique constraint violated
                {
                    // Fallback: if INSERT failed due to the duplicate key (already exists), do UPDATE
                    await UpdateByNameAsync(connection, entity, transaction).ConfigureAwait(false);
                }
            }
        }
        
        private async Task<int> UpdateByNameAsync(OracleConnection connection, ProcessInstancePersistenceEntity entity,
            OracleTransaction transaction = null)
        {
            var parameters = new List<OracleParameter>
            {
                new("processid", OracleDbType.Raw, entity.ProcessId.ToByteArray(), ParameterDirection.Input),
                new("parameterName", OracleDbType.NVarchar2, entity.ParameterName, ParameterDirection.Input),
                new("value", OracleDbType.NClob, entity.Value, ParameterDirection.Input)
            };

            string commandText = $"UPDATE {ObjectName} " +
                                 $"SET {nameof(ProcessInstancePersistenceEntity.Value).ToUpperInvariant()} = :value " +
                                 $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId).ToUpperInvariant()} = :processid " +
                                 $"AND {nameof(ProcessInstancePersistenceEntity.ParameterName).ToUpperInvariant()} = :parameterName";

            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters.ToArray())
                .ConfigureAwait(false);
        }
    }
}
