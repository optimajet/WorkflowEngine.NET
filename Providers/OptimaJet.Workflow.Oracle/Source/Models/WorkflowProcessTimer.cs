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
    public class WorkflowProcessTimer : DbObject<ProcessTimerEntity>
    {
        public WorkflowProcessTimer(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessTimer", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Id), IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.ProcessId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Name)},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.NextExecutionDateTime), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Ignore), Type = OracleDbType.Byte },
                new ColumnInfo {Name = nameof(ProcessTimerEntity.RootProcessId), Type = OracleDbType.Raw},
            });
        }

        public async Task<int> DeleteInactiveByProcessIdAsync(OracleConnection connection, Guid processId, OracleTransaction transaction = null)
        {
            var pProcessId = new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE {nameof(ProcessTimerEntity.ProcessId).ToUpperInvariant()} = :processid " +
                    $"AND {nameof(ProcessTimerEntity.Ignore).ToUpperInvariant()} = 1",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId, List<string> timersIgnoreList = null, OracleTransaction transaction = null)
        {
            var pProcessId = new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input);

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<OracleParameter>() {pProcessId};
                int cnt = 0;
                foreach (string timer in timersIgnoreList)
                {
                    string parameterName = $"ignore{cnt}";
                    parameters.Add($":{parameterName}");
                    sqlParameters.Add(new OracleParameter(parameterName, OracleDbType.NVarchar2, timer, ParameterDirection.Input));
                    cnt++;
                }

                string commandText = $"DELETE FROM {ObjectName} " +
                                     $"WHERE {nameof(ProcessTimerEntity.ProcessId).ToUpperInvariant()} = :processid " +
                                     $"AND {nameof(ProcessTimerEntity.Name).ToUpperInvariant()} " +
                                     $"NOT IN ({String.Join(",", parameters)})";

                return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, sqlParameters.ToArray()).ConfigureAwait(false);
            }

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE {nameof(ProcessTimerEntity.ProcessId).ToUpperInvariant()} = :processid",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity> SelectByProcessIdAndNameAsync(OracleConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(ProcessTimerEntity.ProcessId).ToUpperInvariant()} = :processid " +
                                $"AND {nameof(ProcessTimerEntity.Name).ToUpperInvariant()} = :name";
    
            return (await SelectAsync(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input),
                new OracleParameter("name", OracleDbType.NVarchar2, name, ParameterDirection.Input)).ConfigureAwait(false))
                .FirstOrDefault();
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(ProcessTimerEntity.ProcessId).ToUpperInvariant()} = :processid";

            return await SelectAsync(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input)).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectActiveByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(ProcessTimerEntity.ProcessId).ToUpperInvariant()} = :processid " +
                                $"AND {nameof(ProcessTimerEntity.Ignore).ToUpperInvariant()} = 0";

            return await SelectAsync(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input)).ConfigureAwait(false);
        }

        public async Task<int> SetTimerIgnoreAsync(OracleConnection connection, Guid timerId, OracleTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET {nameof(ProcessTimerEntity.Ignore).ToUpperInvariant()} = 1 " +
                             $"WHERE {nameof(ProcessTimerEntity.Id).ToUpperInvariant()} = :timerid " +
                             $"AND {nameof(ProcessTimerEntity.Ignore).ToUpperInvariant()} = 0";
            
            var p1 = new OracleParameter("timerid", OracleDbType.Raw, timerId.ToByteArray(), ParameterDirection.Input);
            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1).ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity[]> GetTopTimersToExecuteAsync(OracleConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM (SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(ProcessTimerEntity.Ignore).ToUpperInvariant()} = 0 " +
                                $"AND {nameof(ProcessTimerEntity.NextExecutionDateTime)} <= :currentTime " +
                                $"ORDER BY {nameof(ProcessTimerEntity.NextExecutionDateTime)}) " +
                                $"WHERE ROWNUM <= :rowsCount";

            return await SelectAsync(connection, selectText,
                new OracleParameter("currentTime", OracleDbType.Date, now, ParameterDirection.Input),
                new OracleParameter("rowsCount", OracleDbType.Int32, top, ParameterDirection.Input)
            ).ConfigureAwait(false);
        }
    }
}
