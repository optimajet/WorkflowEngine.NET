using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.PostgreSQL.Models
{
    public class WorkflowRuntime : DbObject<RuntimeEntity>
    {
        public WorkflowRuntime(string schemaName, int commandTimeout) : base(schemaName, "WorkflowRuntime", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(RuntimeEntity.RuntimeId), IsKey = true},
                new ColumnInfo {Name = nameof(RuntimeEntity.Lock), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(RuntimeEntity.Status), Type = NpgsqlDbType.Smallint},
                new ColumnInfo {Name = nameof(RuntimeEntity.RestorerId)},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextTimerTime), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextServiceTimerTime), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(RuntimeEntity.LastAliveSignal), Type = NpgsqlDbType.Timestamp}
            });
        }

        public async Task<int> UpdateStatusAsync(NpgsqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"\"{nameof(RuntimeEntity.Status)}\" = @newstatus, " +
                             $"\"{nameof(RuntimeEntity.Lock)}\" = @newlock " +
                             $"WHERE \"{nameof(RuntimeEntity.RuntimeId)}\" = @id " +
                             $"AND \"{nameof(RuntimeEntity.Lock)}\" = @oldlock";

            var p1 = new NpgsqlParameter("newstatus", NpgsqlDbType.Smallint) {Value = (int)status.Status};
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) {Value = status.Lock};
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = status.RuntimeId};
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) {Value = oldLock};

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<int> UpdateRestorerAsync(NpgsqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"\"{nameof(RuntimeEntity.RestorerId)}\" = @restorer, " +
                             $"\"{nameof(RuntimeEntity.Lock)}\" = @newlock " +
                             $"WHERE \"{nameof(RuntimeEntity.RuntimeId)}\" = @id " +
                             $"AND \"{nameof(RuntimeEntity.Lock)}\" = @oldlock";

            var p1 = new NpgsqlParameter("restorer", NpgsqlDbType.Varchar) {Value = status.RestorerId};
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) {Value = status.Lock};
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = status.RuntimeId};
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) {Value = oldLock};

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<bool> MultiServerRuntimesExistAsync(NpgsqlConnection connection)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE \"{nameof(RuntimeEntity.RuntimeId)}\" != @empty " +
                                $"AND \"{nameof(RuntimeEntity.Status)}\" NOT IN (" +
                                $"{(int)RuntimeStatus.Dead}, " +
                                $"{(int)RuntimeStatus.Terminated})";

            RuntimeEntity[] runtimes = await SelectAsync(connection, selectText,
                new NpgsqlParameter("empty", NpgsqlDbType.Varchar) {Value = Guid.Empty.ToString()}).ConfigureAwait(false);

            return runtimes.Length > 0;
        }

        public async Task<int> GetActiveMultiServerRuntimesCountAsync(NpgsqlConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE \"{nameof(RuntimeEntity.RuntimeId)}\" != @current " +
                                $"AND \"{nameof(RuntimeEntity.Status)}\" IN (" +
                                $"{(int)RuntimeStatus.Alive}, " +
                                $"{(int)RuntimeStatus.Restore}, " +
                                $"{(int)RuntimeStatus.SelfRestore})";

            RuntimeEntity[] runtimes =
                await SelectAsync(connection, selectText, new NpgsqlParameter("current", NpgsqlDbType.Varchar) {Value = currentRuntimeId})
                    .ConfigureAwait(false);

            return runtimes.Length;
        }

        public async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(NpgsqlConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                $"WHERE \"{nameof(RuntimeEntity.RuntimeId)}\" = @id";

            RuntimeEntity r = (await SelectAsync(
                    connection,
                    selectText,
                    new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId })
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

        public async Task<int> SendRuntimeLastAliveSignalAsync(NpgsqlConnection connection, string runtimeId, DateTime time)
        {
            string command = $"UPDATE {ObjectName} SET \"LastAliveSignal\" = @time " +
                             $"WHERE \"{nameof(RuntimeEntity.RuntimeId)}\" = @id " +
                             $"AND \"{nameof(RuntimeEntity.Status)}\" IN (" +
                             $"{(int)RuntimeStatus.Alive}," +
                             $"{(int)RuntimeStatus.SelfRestore})";

            var p1 = new NpgsqlParameter("time", NpgsqlDbType.Timestamp) {Value = time};
            var p2 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = runtimeId};

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2).ConfigureAwait(false);
        }

        public async Task<int> UpdateNextTimeAsync(NpgsqlConnection connection, string runtimeId, string nextTimeColumnName, DateTime time,
            NpgsqlTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET " +
                             $"\"{nextTimeColumnName}\" = @time " +
                             $"WHERE \"{nameof(RuntimeEntity.RuntimeId)}\" = @id";

            var p1 = new NpgsqlParameter("time", NpgsqlDbType.Timestamp) {Value = time};
            var p2 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = runtimeId};

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2).ConfigureAwait(false);
        }

        public async Task<DateTime?> GetMaxNextTimeAsync(NpgsqlConnection connection, string runtimeId, string nextTimeColumnName)
        {
            string commandText = $"SELECT MAX(\"{nextTimeColumnName}\") FROM {ObjectName} " +
                                 $"WHERE \"{nameof(RuntimeEntity.Status)}\" = 0 " +
                                 $"AND \"{nameof(RuntimeEntity.RuntimeId)}\" != @id";

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new NpgsqlCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Varchar) {Value = runtimeId});

            object result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            return result as DateTime?;
        }
    }
}
