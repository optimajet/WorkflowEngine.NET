using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;

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
                new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId}, 
                new SqlParameter("parameterName", SqlDbType.NVarChar) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE [{nameof(ProcessInstancePersistenceEntity.ProcessId)}] = @processid " +
                    $"AND [{nameof(ProcessInstancePersistenceEntity.ParameterName)}] = @parameterName",
                    transaction,
                    parameters.ToArray())
                .ConfigureAwait(false);
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
    }
}
