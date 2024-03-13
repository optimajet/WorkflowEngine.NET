using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.SQLite.Models
{
    public class WorkflowRuntime : DbObject<RuntimeEntity>
    {
        public WorkflowRuntime(string schemaName, int commandTimeout) : base(schemaName, "WorkflowRuntime", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(RuntimeEntity.RuntimeId), IsKey = true},
                new ColumnInfo {Name = nameof(RuntimeEntity.Lock), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(RuntimeEntity.Status), Type = DbType.Byte},
                new ColumnInfo {Name = nameof(RuntimeEntity.RestorerId)},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextTimerTime), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextServiceTimerTime), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(RuntimeEntity.LastAliveSignal), Type = DbType.DateTime2}
            });
        }

        public async Task<int> UpdateStatusAsync(SqliteConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"{nameof(RuntimeEntity.Status)} = @newstatus, " +
                             $"{nameof(RuntimeEntity.Lock)} = @newlock " +
                             $"WHERE {nameof(RuntimeEntity.RuntimeId)} = @id " +
                             $"AND {nameof(RuntimeEntity.Lock)} = @oldlock";

            var p1 = new SqliteParameter("newstatus", DbType.Byte) {Value = status.Status};
            var p2 = new SqliteParameter("newlock", DbType.String) {Value = ToDbValue(status.Lock, DbType.Guid)};
            var p3 = new SqliteParameter("id", DbType.String) {Value = status.RuntimeId};
            var p4 = new SqliteParameter("oldlock", DbType.String) {Value = ToDbValue(oldLock, DbType.Guid)};

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<int> UpdateRestorerAsync(SqliteConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"{nameof(RuntimeEntity.RestorerId)} = @restorer, " +
                             $"{nameof(RuntimeEntity.Lock)} = @newlock " +
                             $"WHERE {nameof(RuntimeEntity.RuntimeId)} = @id " +
                             $"AND {nameof(RuntimeEntity.Lock)} = @oldlock";

            var p1 = new SqliteParameter("restorer", DbType.String) {Value = status.RestorerId};
            var p2 = new SqliteParameter("newlock", DbType.String) {Value = ToDbValue(status.Lock, DbType.Guid)};
            var p3 = new SqliteParameter("id", DbType.String) {Value = status.RuntimeId};
            var p4 = new SqliteParameter("oldlock", DbType.String) {Value = ToDbValue(oldLock, DbType.Guid)};

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<bool> MultiServerRuntimesExistAsync(SqliteConnection connection)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(RuntimeEntity.RuntimeId)} != @empty " +
                                $"AND {nameof(RuntimeEntity.Status)} NOT IN (" +
                                $"{(int)RuntimeStatus.Dead}, " +
                                $"{(int)RuntimeStatus.Terminated})";

            RuntimeEntity[] runtimes = await SelectAsync(connection, selectText,
                new SqliteParameter("empty", DbType.String) {Value = Guid.Empty.ToString()}).ConfigureAwait(false);

            return runtimes.Length > 0;
        }

        public async Task<int> GetActiveMultiServerRuntimesCountAsync(SqliteConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(RuntimeEntity.RuntimeId)} != @current " +
                                $"AND {nameof(RuntimeEntity.Status)} IN (" +
                                $"{(int)RuntimeStatus.Alive}, " +
                                $"{(int)RuntimeStatus.Restore}, " +
                                $"{(int)RuntimeStatus.SelfRestore})";

            RuntimeEntity[] runtimes =
                await SelectAsync(connection, selectText, new SqliteParameter("current", DbType.String) {Value = currentRuntimeId})
                    .ConfigureAwait(false);

            return runtimes.Length;
        }

        public async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(SqliteConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE {nameof(RuntimeEntity.RuntimeId)} = @id";

            RuntimeEntity r = (await SelectAsync(
                    connection,
                    selectText,
                    new SqliteParameter("id", DbType.String) { Value = runtimeId })
                .ConfigureAwait(false)).FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel
            {
                Lock = r.Lock,
                RuntimeId = r.RuntimeId,
                Status = r.Status,
                RestorerId = r.RestorerId,
                LastAliveSignal = r.LastAliveSignal,
                NextTimerTime = r.NextTimerTime
            };
        }

        public async Task<int> SendRuntimeLastAliveSignalAsync(SqliteConnection connection, string runtimeId, DateTime time)
        {
            string command = $"UPDATE {ObjectName} SET {nameof(RuntimeEntity.LastAliveSignal)} = @time " +
                             $"WHERE {nameof(RuntimeEntity.RuntimeId)} = @id " +
                             $"AND {nameof(RuntimeEntity.Status)} IN (" +
                             $"{(int)RuntimeStatus.Alive}," +
                             $"{(int)RuntimeStatus.SelfRestore})";

            var p1 = new SqliteParameter("time", DbType.Int64) {Value = ToDbValue(time, DbType.DateTime2)};
            var p2 = new SqliteParameter("id", DbType.String) {Value = runtimeId};

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2).ConfigureAwait(false);
        }

        public async Task<int> UpdateNextTimeAsync(SqliteConnection connection, string runtimeId, string nextTimeColumnName, DateTime time,
            SqliteTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"{nextTimeColumnName} = @time " +
                             $"WHERE {nameof(RuntimeEntity.RuntimeId)} = @id";

            var p1 = new SqliteParameter("time", DbType.Int64) {Value = ToDbValue(time, DbType.DateTime2)};
            var p2 = new SqliteParameter("id", DbType.String) {Value = runtimeId};

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2).ConfigureAwait(false);
        }

        public async Task<DateTime?> GetMaxNextTimeAsync(SqliteConnection connection, string runtimeId, string nextTimeColumnName)
        {
            string commandText = $"SELECT MAX({nextTimeColumnName}) FROM {ObjectName} " +
                                 $"WHERE {nameof(RuntimeEntity.Status)} = 0 " +
                                 $"AND {nameof(RuntimeEntity.RuntimeId)} != @id";

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new SqliteCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new SqliteParameter("id", DbType.String) {Value = runtimeId});

            object result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            return result as DateTime?;
        }
    }
}
