using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.PostgreSQL.Models
{
    public class WorkflowRuntime : DbObject<WorkflowRuntime>
    {
        static WorkflowRuntime()
        {
            DbTableName = "WorkflowRuntime";
        }

        public WorkflowRuntime()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "RuntimeId", IsKey = true},
                new ColumnInfo {Name = "Lock", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = "Status", Type = NpgsqlDbType.Smallint},
                new ColumnInfo {Name = "RestorerId"},
                new ColumnInfo {Name = "NextTimerTime", Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = "NextServiceTimerTime", Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = "LastAliveSignal", Type = NpgsqlDbType.Timestamp}
            });
        }

        public string RuntimeId { get; set; }
        public Guid Lock { get; set; }
        public RuntimeStatus Status { get; set; }
        public string RestorerId { get; set; }
        public DateTime? NextTimerTime { get; set; }
        public DateTime? NextServiceTimerTime { get; set; }
        public DateTime? LastAliveSignal { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "RuntimeId":
                    return RuntimeId;
                case "Lock":
                    return Lock;
                case "Status":
                    return (short)Status;
                case "RestorerId":
                    return RestorerId;
                case "NextTimerTime":
                    return NextTimerTime;
                case "NextServiceTimerTime":
                    return NextServiceTimerTime;
                case "LastAliveSignal":
                    return LastAliveSignal;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Lock":
                    Lock = (Guid)value;
                    break;
                case "RuntimeId":
                    RuntimeId = value as string;
                    break;
                case "Status":
                    Status = (RuntimeStatus)(short)value;
                    break;
                case "RestorerId":
                    RestorerId = value as string;
                    break;
                case "NextTimerTime":
                    NextTimerTime = value as DateTime?;
                    break;
                case "NextServiceTimerTime":
                    NextServiceTimerTime = value as DateTime?;
                    break;
                case "LastAliveSignal":
                    LastAliveSignal = value as DateTime?;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public static async Task<int> UpdateStatusAsync(NpgsqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET \"Status\" = @newstatus, \"Lock\" = @newlock WHERE \"RuntimeId\" = @id AND \"Lock\" = @oldlock";
            var p1 = new NpgsqlParameter("newstatus", NpgsqlDbType.Smallint) { Value = (int)status.Status };
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = status.Lock };
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = status.RuntimeId };
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public static async Task<int> UpdateRestorerAsync(NpgsqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET \"RestorerId\" = @restorer, \"Lock\" = @newlock WHERE \"RuntimeId\" = @id AND \"Lock\" = @oldlock";
            var p1 = new NpgsqlParameter("restorer", NpgsqlDbType.Varchar) { Value = status.RestorerId };
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = status.Lock };
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = status.RuntimeId };
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public static async Task<bool> MultiServerRuntimesExistAsync(NpgsqlConnection connection)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"RuntimeId\" != @empty AND \"Status\" NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated})";
            WorkflowRuntime[] runtimes = await SelectAsync(connection, selectText, new NpgsqlParameter("empty", NpgsqlDbType.Varchar) { Value = Guid.Empty.ToString() }).ConfigureAwait(false);

            return runtimes.Length > 0;
        }

        public static async Task<int> GetActiveMultiServerRuntimesCountAsync(NpgsqlConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"RuntimeId\" != @current AND \"Status\" IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            WorkflowRuntime[] runtimes = await SelectAsync(connection, selectText, new NpgsqlParameter("current", NpgsqlDbType.Varchar) { Value = currentRuntimeId }).ConfigureAwait(false);

            return runtimes.Length;
        }

        public static async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(NpgsqlConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"RuntimeId\" = @id";
            WorkflowRuntime r = (await SelectAsync(connection, selectText, new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId }).ConfigureAwait(false)).FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.Lock, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime };
        }

        public static async Task<int> SendRuntimeLastAliveSignalAsync(NpgsqlConnection connection, string runtimeId, DateTime time)
        {
            string command = $"UPDATE {ObjectName} SET \"LastAliveSignal\" = @time WHERE \"RuntimeId\" = @id AND \"Status\" IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            var p1 = new NpgsqlParameter("time", NpgsqlDbType.Timestamp) { Value = time };
            var p2 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2).ConfigureAwait(false);
        }

        public static async Task<int> UpdateNextTimeAsync(NpgsqlConnection connection, string runtimeId, string nextTimeColumnName, DateTime time,
            NpgsqlTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET \"{nextTimeColumnName}\" = @time WHERE \"RuntimeId\" = @id";
            var p1 = new NpgsqlParameter("time", NpgsqlDbType.Timestamp) { Value = time };
            var p2 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2).ConfigureAwait(false);
        }

        public static async Task<DateTime?> GetMaxNextTimeAsync(NpgsqlConnection connection, string runtimeId, string nextTimeColumnName)
        {
            string commandText = $"SELECT MAX(\"{nextTimeColumnName}\") FROM {ObjectName} WHERE \"Status\" = 0 AND \"RuntimeId\" != @id";

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new NpgsqlCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId });

            object result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            return result as DateTime?;
        }
    }
}
