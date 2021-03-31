using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.MySQL.Models
{
    public class WorkflowRuntime : DbObject<WorkflowRuntime>
    {
        static WorkflowRuntime()
        {
            DbTableName = "workflowruntime";
        }

        public WorkflowRuntime()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "RuntimeId", IsKey = true, Type = MySqlDbType.VarString, Size = 450},
                new ColumnInfo {Name = "Lock", Type = MySqlDbType.Binary},
                new ColumnInfo {Name = "Status", Type = MySqlDbType.Byte},
                new ColumnInfo {Name = "RestorerId", Type = MySqlDbType.VarString, Size = 1024},
                new ColumnInfo {Name = "NextTimerTime", Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = "NextServiceTimerTime", Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = "LastAliveSignal", Type = MySqlDbType.DateTime}
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
                    return Lock.ToByteArray();
                case "Status":
                    return (sbyte)Status;
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
                    Lock = new Guid((byte[])value);
                    break;
                case "RuntimeId":
                    RuntimeId = value as string;
                    break;
                case "Status":
                    Status = (RuntimeStatus)(sbyte)value;
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
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<int> UpdateStatusAsync(MySqlConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = $"UPDATE {DbTableName} SET `Status` = @newstatus, `Lock` = @newlock WHERE `RuntimeId` = @id AND `Lock` = @oldlock";
            var p1 = new MySqlParameter("newstatus", MySqlDbType.Byte) { Value = runtime.Status };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = runtime.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtime.RuntimeId };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public static async Task<int> UpdateRestorerAsync(MySqlConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = $"UPDATE {DbTableName} SET `RestorerId` = @restorer, `Lock` = @newlock WHERE `RuntimeId` = @id AND `Lock` = @oldlock";
            var p1 = new MySqlParameter("restorer", MySqlDbType.VarString) { Value = runtime.RestorerId };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = runtime.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtime.RuntimeId };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public static async Task<bool> MultiServerRuntimesExistAsync(MySqlConnection connection)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `RuntimeId` != @empty AND `Status` NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated})";
            WorkflowRuntime[] runtimes = await SelectAsync(connection, selectText, new MySqlParameter("empty", MySqlDbType.VarString) { Value = Guid.Empty.ToString() }).ConfigureAwait(false);

            return runtimes.Length > 0;
        }

        public static async Task<int> GetActiveMultiServerRuntimesCountAsync(MySqlConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `RuntimeId` != @current AND `Status` IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            WorkflowRuntime[] runtimes = await SelectAsync(connection, selectText, new MySqlParameter("current", MySqlDbType.VarString) { Value = currentRuntimeId }).ConfigureAwait(false);

            return runtimes.Length;
        }

        public static async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(MySqlConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `RuntimeId` = @id";
            WorkflowRuntime r = (await SelectAsync(connection, selectText, new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId }).ConfigureAwait(false)).FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.Lock, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime };
        }

        public static async Task<int> SendRuntimeLastAliveSignalAsync(MySqlConnection connection, string runtimeId, DateTime time)
        {
            string command = $"UPDATE {DbTableName} SET `LastAliveSignal` = @time WHERE `RuntimeId` = @id AND `Status` IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            var p1 = new MySqlParameter("time", MySqlDbType.DateTime) { Value = time };
            var p2 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2).ConfigureAwait(false);
        }

        public static async Task<int> UpdateNextTimeAsync(MySqlConnection connection, string runtimeId, string nextTimeColumnName, DateTime time,
            MySqlTransaction transaction = null)
        {
            string command = $"UPDATE {DbTableName} SET `{nextTimeColumnName}` = @time WHERE `RuntimeId` = @id";
            var p1 = new MySqlParameter("time", MySqlDbType.DateTime) { Value = time };
            var p2 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2).ConfigureAwait(false);
        }

        public static async Task<DateTime?> GetMaxNextTimeAsync(MySqlConnection connection, string runtimeId, string nextTimeColumnName)
        {
            string commandText = $"SELECT MAX(`{nextTimeColumnName}`) FROM {DbTableName} WHERE `Status` = 0 AND `RuntimeId` != @id";

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new MySqlCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId });

            object result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            return result as DateTime?;
        }
    }
}
