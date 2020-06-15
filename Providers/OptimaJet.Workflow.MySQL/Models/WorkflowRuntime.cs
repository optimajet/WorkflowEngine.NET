using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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

        public static int UpdateStatus(MySqlConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = String.Format("UPDATE {0} SET `Status` = @newstatus, `Lock` = @newlock WHERE `RuntimeId` = @id AND `Lock` = @oldlock", DbTableName);
            var p1 = new MySqlParameter("newstatus", MySqlDbType.Byte) { Value = runtime.Status };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = runtime.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtime.RuntimeId };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }

        public static int UpdateRestorer(MySqlConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            var command = String.Format("UPDATE {0} SET `RestorerId` = @restorer, `Lock` = @newlock WHERE `RuntimeId` = @id AND `Lock` = @oldlock", DbTableName);
            var p1 = new MySqlParameter("restorer", MySqlDbType.VarString) { Value = runtime.RestorerId };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = runtime.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtime.RuntimeId };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }

        public static bool MultiServerRuntimesExist(MySqlConnection connection)
        {
            var selectText = $"SELECT * FROM {DbTableName} WHERE `RuntimeId` != @empty OR `Status` NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated}, {(int)RuntimeStatus.Single})";
            var runtimes = Select(connection, selectText, new MySqlParameter("empty", MySqlDbType.VarString) { Value = Guid.Empty.ToString() });

            return runtimes.Length > 0;
        }

        public static int SingleServerRuntimesCount(MySqlConnection connection)
        {
            var selectText = $"SELECT * FROM {DbTableName} WHERE `RuntimeId` = @empty AND `Status` = {(int)RuntimeStatus.Single}";
            var runtimes = Select(connection, selectText, new MySqlParameter("empty", MySqlDbType.VarString) { Value = Guid.Empty.ToString() });

            return runtimes.Length;
        }

        public static int ActiveMultiServerRuntimesCount(MySqlConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `RuntimeId` != @current AND `Status` IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            WorkflowRuntime[] runtimes = Select(connection, selectText, new MySqlParameter("current", MySqlDbType.VarString) { Value = currentRuntimeId });

            return runtimes.Length;
        }

        public static WorkflowRuntimeModel GetWorkflowRuntimeStatus(MySqlConnection connection, string runtimeId)
        {
            string selectText = String.Format("SELECT * FROM {0} WHERE `RuntimeId` = @id", DbTableName);
            WorkflowRuntime r = Select(connection, selectText, new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId }).FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.Lock, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime };
        }

        public static int SendRuntimeLastAliveSignal(MySqlConnection connection, string runtimeId, DateTime time)
        {
            var command = $"UPDATE {DbTableName} SET `LastAliveSignal` = @time WHERE `RuntimeId` = @id AND `Status` IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            var p1 = new MySqlParameter("time", MySqlDbType.DateTime) { Value = time };
            var p2 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId };

            return ExecuteCommand(connection, command, p1, p2);
        }

        public static int UpdateNextTime(MySqlConnection connection, string runtimeId, string nextTimeColumnName, DateTime time,
            MySqlTransaction transaction = null)
        {
            var command = String.Format("UPDATE {0} SET `{1}` = @time WHERE `RuntimeId` = @id", DbTableName, nextTimeColumnName);
            var p1 = new MySqlParameter("time", MySqlDbType.DateTime) { Value = time };
            var p2 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId };

            return ExecuteCommand(connection, command, transaction, p1, p2);
        }

        public static DateTime? GetMaxNextTime(MySqlConnection connection, string runtimeId, string nextTimeColumnName)
        {
            var commandText = String.Format("SELECT MAX(`{1}`) FROM {0} WHERE `Status` = 0 AND `RuntimeId` != @id", DbTableName, nextTimeColumnName);

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new MySqlCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId });

            object result = command.ExecuteScalar();

            if (result == null)
            {
                return null;
            }

            return result as DateTime?;
        }
    }
}
