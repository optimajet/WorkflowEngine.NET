using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessTimer : DbObject<ProcessTimerEntity>
    {
        public WorkflowProcessTimer(int commandTimeout) : base("workflowprocesstimer", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.ProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Name)},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.NextExecutionDateTime), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Ignore), Type = MySqlDbType.Bit},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.RootProcessId), Type = MySqlDbType.Binary},
            });
        }

        public async Task<int> DeleteInactiveByProcessIdAsync(MySqlConnection connection, Guid processId,
            MySqlTransaction transaction = null)
        {
            var pProcessId = new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()};

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {DbTableName} " +
                    $"WHERE `{nameof(ProcessTimerEntity.ProcessId)}` = @processid " +
                    $"AND `{nameof(ProcessTimerEntity.Ignore)}` = 1",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId,
            List<string> timersIgnoreList = null, MySqlTransaction transaction = null)
        {
            var pProcessId = new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()};

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<MySqlParameter>() {pProcessId};
                int cnt = 0;
                foreach (string timer in timersIgnoreList)
                {
                    string parameterName = $"ignore{cnt}";
                    parameters.Add($"@{parameterName}");
                    sqlParameters.Add(new MySqlParameter(parameterName, MySqlDbType.VarString) {Value = timer});
                    cnt++;
                }

                string commandText = $"DELETE FROM {DbTableName} " +
                                     $"WHERE `{nameof(ProcessTimerEntity.ProcessId)}` = @processid " +
                                     $"AND `{nameof(ProcessTimerEntity.Name)}` not in ({string.Join(",", parameters)})";

                return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, sqlParameters.ToArray()).ConfigureAwait(false);
            }

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {DbTableName} " +
                    $"WHERE `{nameof(ProcessTimerEntity.ProcessId)}` = @processid",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity> SelectByProcessIdAndNameAsync(MySqlConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {DbTableName} " +
                                $"WHERE `{nameof(ProcessTimerEntity.ProcessId)}` = @processid " +
                                $"AND `{nameof(ProcessTimerEntity.Name)}` = @name";

            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()};

            var p2 = new MySqlParameter("name", MySqlDbType.VarString) {Value = name};
            return (await SelectAsync(connection, selectText, p1, p2).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {DbTableName} " +
                                $"WHERE `{nameof(ProcessTimerEntity.ProcessId)}` = @processid";

            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectActiveByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {DbTableName} " +
                                $"WHERE `{nameof(ProcessTimerEntity.ProcessId)}` = @processid " +
                                $"AND `{nameof(ProcessTimerEntity.Ignore)}` = 0";

            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public async Task<int> SetTimerIgnoreAsync(MySqlConnection connection, Guid timerId, MySqlTransaction transaction = null)
        {
            string command = $"UPDATE {DbTableName} SET " +
                             $"`{nameof(ProcessTimerEntity.Ignore)}` = 1 " +
                             $"WHERE `{nameof(ProcessTimerEntity.Id)}` = @timerid " +
                             $"AND `{nameof(ProcessTimerEntity.Ignore)}` = 0";

            var p1 = new MySqlParameter("timerid", MySqlDbType.Binary) {Value = timerId.ToByteArray()};
            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1).ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity[]> GetTopTimersToExecuteAsync(MySqlConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM {DbTableName} " +
                                $"WHERE `{nameof(ProcessTimerEntity.Ignore)}` = 0 " +
                                $"AND `{nameof(ProcessTimerEntity.NextExecutionDateTime)}` <= @currentTime " +
                                $"ORDER BY `{nameof(ProcessTimerEntity.NextExecutionDateTime)}` LIMIT {top}";

            var p1 = new MySqlParameter("currentTime", MySqlDbType.DateTime) {Value = now};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
