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
            List<OracleParameter> parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input));
            parameters.Add(new OracleParameter("parameterName", OracleDbType.NVarchar2, parameterName, ParameterDirection.Input));

            return await ExecuteCommandNonQueryAsync(
                connection,
                $"DELETE FROM {ObjectName} " + 
                $"WHERE {nameof(ProcessInstancePersistenceEntity.ProcessId).ToUpperInvariant()} = :processid",
                parameters.ToArray()).ConfigureAwait(false);
        }
    }
}
