using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessInstancePersistence : DbObject<ProcessInstancePersistenceEntity>
    {
        public WorkflowProcessInstancePersistence(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstancePersistence", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ProcessId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.ParameterName)},
                new ColumnInfo {Name = nameof(ProcessInstancePersistenceEntity.Value), Type = SqlDbType.NVarChar, Size = -1}
            });
        }

        public async Task<ProcessInstancePersistenceEntity[]> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE [{nameof(ProcessInstancePersistenceEntity.ProcessId)}] = @processid";
            var p = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }

        public async Task<ProcessInstancePersistenceEntity> SelectByNameAsync(SqlConnection connection, Guid processId, string parameterName)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE [{nameof(ProcessInstancePersistenceEntity.ProcessId)}] = @processid " + 
                                $"AND [{nameof(ProcessInstancePersistenceEntity.ParameterName)}] = @parameterName";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId}, 
                new SqlParameter("parameterName", SqlDbType.NVarChar) {Value = parameterName}
            };

            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            var p = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE [{nameof(ProcessInstancePersistenceEntity.ProcessId)}] = @processid",
                    transaction,
                    p)
                .ConfigureAwait(false);
        }
        public async Task<int> DeleteByNameAsync(SqlConnection connection, Guid processId,string parameterName, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new("processid", SqlDbType.UniqueIdentifier) {Value = processId}, 
                new("parameterName", SqlDbType.NVarChar) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE [{nameof(ProcessInstancePersistenceEntity.ProcessId)}] = @processid " +
                    $"AND [{nameof(ProcessInstancePersistenceEntity.ParameterName)}] = @parameterName",
                    transaction,
                    parameters.ToArray())
                .ConfigureAwait(false);
        }

       public async Task UpsertAsync(SqlConnection connection, ProcessInstancePersistenceEntity entity,
            bool preferUpdateFirst, SqlTransaction transaction)
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
                catch (PersistenceProviderQueryException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 })
                {
                    // Fallback: if INSERT failed due to the duplicate key (already exists), do UPDATE
                    await UpdateByNameAsync(connection, entity, transaction).ConfigureAwait(false);
                }
            }
        }

        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add(nameof(ProcessInstancePersistenceEntity.Id), typeof(Guid));
            dt.Columns.Add(nameof(ProcessInstancePersistenceEntity.ProcessId), typeof(Guid));
            dt.Columns.Add(nameof(ProcessInstancePersistenceEntity.ParameterName), typeof(string));
            dt.Columns.Add(nameof(ProcessInstancePersistenceEntity.Value), typeof(string));
            return dt;
        }
        
        private async Task<int> UpdateByNameAsync(SqlConnection connection, ProcessInstancePersistenceEntity entity, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new("processid", SqlDbType.UniqueIdentifier) {Value = entity.ProcessId},
                new("parameterName", SqlDbType.NVarChar) {Value = entity.ParameterName},
                new("value", SqlDbType.NVarChar, -1) {Value = (object)entity.Value ?? DBNull.Value}
            };

            string updateCommand = $@"
                UPDATE {ObjectName}
                SET [{nameof(ProcessInstancePersistenceEntity.Value)}] = @value
                WHERE [{nameof(ProcessInstancePersistenceEntity.ProcessId)}] = @processid 
                  AND [{nameof(ProcessInstancePersistenceEntity.ParameterName)}] = @parameterName";

            return await ExecuteCommandNonQueryAsync(connection, updateCommand, transaction, parameters.ToArray())
                .ConfigureAwait(false);
        }
    }
}
