using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.SQLite.Models
{
    /// <summary>
    /// Database object for workflow runtime status and multi-server coordination operations.
    /// </summary>
    public class WorkflowRuntime : DbObject<RuntimeEntity>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WorkflowRuntime"/>.
        /// </summary>
        /// <param name="schemaName">The database schema name.</param>
        /// <param name="commandTimeout">The command timeout in seconds.</param>
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

        /// <summary>
        /// Updates the runtime status using optimistic locking.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="status">The runtime model with the new status.</param>
        /// <param name="oldLock">The current lock value for optimistic concurrency.</param>
        /// <returns>The number of rows affected.</returns>
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

        /// <summary>
        /// Updates the runtime restorer identifier using optimistic locking.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="status">The runtime model with the new restorer.</param>
        /// <param name="oldLock">The current lock value for optimistic concurrency.</param>
        /// <returns>The number of rows affected.</returns>
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

        /// <summary>
        /// Checks whether multiple active workflow runtimes exist in the database.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <returns><c>true</c> if more than one active runtime exists; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Returns the count of active runtimes excluding the current one.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="currentRuntimeId">The current runtime identifier to exclude.</param>
        /// <returns>The number of other active runtimes.</returns>
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

        /// <summary>
        /// Gets the runtime status model for the specified runtime identifier.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="runtimeId">The runtime identifier.</param>
        /// <returns>The runtime model, or <c>null</c> if not found.</returns>
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

        /// <summary>
        /// Updates the last alive signal timestamp for the specified runtime.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="runtimeId">The runtime identifier.</param>
        /// <param name="time">The timestamp of the alive signal.</param>
        /// <returns>The number of rows affected.</returns>
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

        /// <summary>
        /// Updates the next timer time column for the specified runtime.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="runtimeId">The runtime identifier.</param>
        /// <param name="nextTimeColumnName">The name of the next time column to update.</param>
        /// <param name="time">The new next time value.</param>
        /// <param name="transaction">An optional transaction.</param>
        /// <returns>The number of rows affected.</returns>
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

        /// <summary>
        /// Gets the maximum next timer time across all other active runtimes.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="runtimeId">The current runtime identifier to exclude.</param>
        /// <param name="nextTimeColumnName">The name of the next time column to query.</param>
        /// <returns>The maximum next time, or <c>null</c> if no other active runtimes exist.</returns>
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
