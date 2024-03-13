using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    public class WorkflowProcessTimer : DbObject<ProcessTimerEntity>
    {
        public WorkflowProcessTimer(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessTimer", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.ProcessId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Name)},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.NextExecutionDateTime), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Ignore), Type = DbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.RootProcessId), Type = DbType.Guid},
            });
        }

        public async Task<int> DeleteInactiveByProcessIdAsync(SqliteConnection connection, Guid processId, SqliteTransaction transaction = null)
        {
            var pProcessId = new SqliteParameter("processid", DbType.String) {Value = ToDbValue(processId, DbType.Guid)};

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE {nameof(ProcessTimerEntity.ProcessId)} = @processid " +
                    $"AND {nameof(ProcessTimerEntity.Ignore)} = TRUE",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByProcessIdAsync(SqliteConnection connection, Guid processId, List<string> timersIgnoreList = null, SqliteTransaction transaction = null)
        {
            var pProcessId = new SqliteParameter("processid", DbType.String) {Value = ToDbValue(processId, DbType.Guid)};

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<SqliteParameter> {pProcessId};
                int cnt = 0;
                foreach (string timer in timersIgnoreList)
                {
                    string parameterName = $"ignore{cnt}";
                    parameters.Add($"@{parameterName}");
                    sqlParameters.Add(new SqliteParameter(parameterName, DbType.String) {Value = timer});
                    cnt++;
                }

                string commandText = $"DELETE FROM {ObjectName} " +
                                     $"WHERE {nameof(ProcessTimerEntity.ProcessId)} = @processid " +
                                     $"AND {nameof(ProcessTimerEntity.Name)} NOT IN ({String.Join(",", parameters)})";

                return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, sqlParameters.ToArray()).ConfigureAwait(false);
            }

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE {nameof(ProcessTimerEntity.ProcessId)} = @processid",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity> SelectByProcessIdAndNameAsync(SqliteConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(ProcessTimerEntity.ProcessId)} = @processid " +
                                $"AND {nameof(ProcessTimerEntity.Name)} = @name";
            var p1 = new SqliteParameter("processid", DbType.String) {Value = ToDbValue(processId, DbType.Guid)};
            var p2 = new SqliteParameter("name", DbType.String) {Value = name};
            return (await SelectAsync(connection, selectText, p1, p2).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectByProcessIdAsync(SqliteConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(ProcessTimerEntity.ProcessId)} = @processid";

            var p1 = new SqliteParameter("processid", DbType.String) {Value = ToDbValue(processId, DbType.Guid)};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectActiveByProcessIdAsync(SqliteConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(ProcessTimerEntity.ProcessId)} = @processid " +
                                $"AND {nameof(ProcessTimerEntity.Ignore)} = FALSE";

            var p1 = new SqliteParameter("processid", DbType.String) {Value = ToDbValue(processId, DbType.Guid)};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public async Task<int> SetTimerIgnoreAsync(SqliteConnection connection, Guid timerId)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"{nameof(ProcessTimerEntity.Ignore)} = TRUE " +
                             $"WHERE {nameof(ProcessTimerEntity.Id)} = @timerid " +
                             $"AND {nameof(ProcessTimerEntity.Ignore)} = FALSE";
            var p1 = new SqliteParameter("timerid", DbType.String) {Value = ToDbValue(timerId, DbType.Guid)};
            return await ExecuteCommandNonQueryAsync(connection, command, p1).ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity[]> GetTopTimersToExecuteAsync(SqliteConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(ProcessTimerEntity.Ignore)} = FALSE " +
                                $"AND {nameof(ProcessTimerEntity.NextExecutionDateTime)} <= @currentTime " +
                                $"ORDER BY {nameof(ProcessTimerEntity.NextExecutionDateTime)} LIMIT {top}";

            var p1 = new SqliteParameter("currentTime", DbType.Int64) {Value = ToDbValue(now, DbType.DateTime2)};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
