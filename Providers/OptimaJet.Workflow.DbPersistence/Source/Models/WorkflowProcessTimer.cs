using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessTimer : DbObject<ProcessTimerEntity>
    {
        public WorkflowProcessTimer(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessTimer", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.ProcessId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Name)},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.NextExecutionDateTime), Type = SqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Ignore), Type = SqlDbType.Bit},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.RootProcessId), Type = SqlDbType.UniqueIdentifier},
            });
        }

        public async Task<int> DeleteInactiveByProcessIdAsync(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE [{nameof(ProcessTimerEntity.ProcessId)}] = @processid " +
                    $"AND [{nameof(ProcessTimerEntity.Ignore)}] = 1",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId, List<string> timersIgnoreList = null,
            SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<SqlParameter> {pProcessId};
                int cnt = 0;
                foreach (string timer in timersIgnoreList)
                {
                    string parameterName = $"ignore{cnt}";
                    parameters.Add($"@{parameterName}");
                    sqlParameters.Add(new SqlParameter(parameterName, SqlDbType.NVarChar) {Value = timer});
                    cnt++;
                }

                string commandText = $"DELETE FROM {ObjectName} " +
                                     $"WHERE [{nameof(ProcessTimerEntity.ProcessId)}] = @processid " +
                                     $"AND [{nameof(ProcessTimerEntity.Name)}] NOT IN ({String.Join(",", parameters)})";

                try
                {
                    return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, sqlParameters.ToArray())
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw ex.RethrowAllowedIfRetrievable();
                }
            }

            try
            {
                return await ExecuteCommandNonQueryAsync(connection,
                        $"DELETE FROM {ObjectName} WHERE [{nameof(ProcessTimerEntity.ProcessId)}] = @processid",
                        transaction,
                        pProcessId)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.RethrowAllowedIfRetrievable();
            }
        }

        public async Task<ProcessTimerEntity> SelectByProcessIdAndNameAsync(SqlConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE [{nameof(ProcessTimerEntity.ProcessId)}] = @processid " +
                                $"AND [{nameof(ProcessTimerEntity.Name)}] = @name";

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            var p2 = new SqlParameter("name", SqlDbType.NVarChar) {Value = name};

            return (await SelectAsync(connection, selectText, p1, p2).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE [{nameof(ProcessTimerEntity.ProcessId)}] = @processid";

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectActiveByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE [{nameof(ProcessTimerEntity.ProcessId)}] = @processid " +
                                $"AND [{nameof(ProcessTimerEntity.Ignore)}] = 0";

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public async Task<int> SetTimerIgnoreAsync(SqlConnection connection, Guid timerId)
        {
            // string command = $"UPDATE {ObjectName} SET [Ignore] = 1 WHERE [Id] = @timerid AND [Ignore] = 0";
            string command = $"UPDATE {ObjectName} SET [{nameof(ProcessTimerEntity.Ignore)}] = 1 " +
                             $"WHERE [{nameof(ProcessTimerEntity.Id)}] = @timerid " +
                             $"AND [{nameof(ProcessTimerEntity.Ignore)}] = 0";

            var p1 = new SqlParameter("timerid", SqlDbType.UniqueIdentifier) {Value = timerId};

            return await ExecuteCommandNonQueryAsync(connection, command, p1).ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity[]> GetTopTimersToExecuteAsync(SqlConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT TOP {top} * FROM {ObjectName}" +
                                $"WHERE [{nameof(ProcessTimerEntity.Ignore)}] = 0 " +
                                $"AND [{nameof(ProcessTimerEntity.NextExecutionDateTime)}] <= @currentTime " +
                                $"ORDER BY [{nameof(ProcessTimerEntity.NextExecutionDateTime)}]";

            var p1 = new SqlParameter("currentTime", SqlDbType.DateTime) {Value = now};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add(nameof(ProcessTimerEntity.Id), typeof(Guid));
            dt.Columns.Add(nameof(ProcessTimerEntity.ProcessId), typeof(Guid));
            dt.Columns.Add(nameof(ProcessTimerEntity.RootProcessId), typeof(Guid));
            dt.Columns.Add(nameof(ProcessTimerEntity.Name), typeof(string));
            dt.Columns.Add(nameof(ProcessTimerEntity.NextExecutionDateTime), typeof(DateTime));
            dt.Columns.Add(nameof(ProcessTimerEntity.Ignore), typeof(bool));
            return dt;
        }
    }
}
