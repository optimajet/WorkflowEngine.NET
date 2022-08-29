using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessTimer : DbObject<ProcessTimerEntity>
    {
        public WorkflowProcessTimer(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessTimer", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.ProcessId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Name)},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.NextExecutionDateTime), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.Ignore), Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessTimerEntity.RootProcessId), Type = NpgsqlDbType.Uuid},
            });
        }

        public async Task<int> DeleteInactiveByProcessIdAsync(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction = null)
        {
            var pProcessId = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE \"{nameof(ProcessTimerEntity.ProcessId)}\" = @processid " +
                    $"AND \"{nameof(ProcessTimerEntity.Ignore)}\" = TRUE",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId, List<string> timersIgnoreList = null, NpgsqlTransaction transaction = null)
        {
            var pProcessId = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                var pTimerIgnoreList = new NpgsqlParameter("timerIgnoreList", NpgsqlDbType.Array | NpgsqlDbType.Varchar) //-V3059
                {
                    Value = timersIgnoreList.ToArray()
                };

                return await ExecuteCommandNonQueryAsync(connection,
                        $"DELETE FROM {ObjectName} " +
                        $"WHERE \"{nameof(ProcessTimerEntity.ProcessId)}\" = @processid " +
                        $"AND \"{nameof(ProcessTimerEntity.Name)}\" != ALL(@timerIgnoreList)",
                        transaction,
                        pProcessId,
                        pTimerIgnoreList)
                    .ConfigureAwait(false);
            }

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} " +
                    $"WHERE \"{nameof(ProcessTimerEntity.ProcessId)}\" = @processid",
                    transaction,
                    pProcessId)
                .ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity> SelectByProcessIdAndNameAsync(NpgsqlConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE \"{nameof(ProcessTimerEntity.ProcessId)}\" = @processid " +
                                $"AND \"{nameof(ProcessTimerEntity.Name)}\" = @name";
            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};
            var p2 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) {Value = name};
            return (await SelectAsync(connection, selectText, p1, p2).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectByProcessIdAsync(NpgsqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE \"{nameof(ProcessTimerEntity.ProcessId)}\" = @processid";

            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ProcessTimerEntity>> SelectActiveByProcessIdAsync(NpgsqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE \"{nameof(ProcessTimerEntity.ProcessId)}\" = @processid " +
                                $"AND \"{nameof(ProcessTimerEntity.Ignore)}\" = FALSE";

            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public async Task<int> SetTimerIgnoreAsync(NpgsqlConnection connection, Guid timerId)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"\"{nameof(ProcessTimerEntity.Ignore)}\" = TRUE " +
                             $"WHERE \"{nameof(ProcessTimerEntity.Id)}\" = @timerid " +
                             $"AND \"{nameof(ProcessTimerEntity.Ignore)}\" = FALSE";
            var p1 = new NpgsqlParameter("timerid", NpgsqlDbType.Uuid) {Value = timerId};
            return await ExecuteCommandNonQueryAsync(connection, command, p1).ConfigureAwait(false);
        }

        public async Task<ProcessTimerEntity[]> GetTopTimersToExecuteAsync(NpgsqlConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE \"{nameof(ProcessTimerEntity.Ignore)}\" = FALSE " +
                                $"AND \"{nameof(ProcessTimerEntity.NextExecutionDateTime)}\" <= @currentTime " +
                                $"ORDER BY \"{nameof(ProcessTimerEntity.NextExecutionDateTime)}\" LIMIT {top}";

            var p1 = new NpgsqlParameter("currentTime", NpgsqlDbType.Timestamp) {Value = now};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
