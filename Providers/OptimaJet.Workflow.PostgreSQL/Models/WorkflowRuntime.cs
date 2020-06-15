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
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int UpdateStatus(NpgsqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            var command = String.Format("UPDATE {0} SET \"Status\" = @newstatus, \"Lock\" = @newlock WHERE \"RuntimeId\" = @id AND \"Lock\" = @oldlock", ObjectName);
            var p1 = new NpgsqlParameter("newstatus", NpgsqlDbType.Smallint) { Value = (int)status.Status };
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = status.Lock };
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = status.RuntimeId };
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }

        public static int UpdateRestorer(NpgsqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            var command = String.Format("UPDATE {0} SET \"RestorerId\" = @restorer, \"Lock\" = @newlock WHERE \"RuntimeId\" = @id AND \"Lock\" = @oldlock", ObjectName);
            var p1 = new NpgsqlParameter("restorer", NpgsqlDbType.Varchar) { Value = status.RestorerId };
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = status.Lock };
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = status.RuntimeId };
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }

        public static bool MultiServerRuntimesExist(NpgsqlConnection connection)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE \"RuntimeId\" != @empty OR \"Status\" NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated}, {(int)RuntimeStatus.Single})";
            var runtimes = Select(connection, selectText, new NpgsqlParameter("empty", NpgsqlDbType.Varchar) { Value = Guid.Empty.ToString() });

            return runtimes.Length > 0;
        }

        public static int SingleServerRuntimesCount(NpgsqlConnection connection)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE \"RuntimeId\" = @empty AND \"Status\" = {(int)RuntimeStatus.Single}";
            var runtimes = Select(connection, selectText, new NpgsqlParameter("empty", NpgsqlDbType.Varchar) { Value = Guid.Empty.ToString() });

            return runtimes.Length;
        }

        public static int ActiveMultiServerRuntimesCount(NpgsqlConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"RuntimeId\" != @current AND \"Status\" IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            var runtimes = Select(connection, selectText, new NpgsqlParameter("current", NpgsqlDbType.Varchar) { Value = currentRuntimeId });

            return runtimes.Length;
        }

        public static WorkflowRuntimeModel GetWorkflowRuntimeStatus(NpgsqlConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"RuntimeId\" = @id";
            WorkflowRuntime r = Select(connection, selectText, new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId }).FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.Lock, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime };
        }

        public static int SendRuntimeLastAliveSignal(NpgsqlConnection connection, string runtimeId, DateTime time)
        {
            var command = $"UPDATE {ObjectName} SET \"LastAliveSignal\" = @time WHERE \"RuntimeId\" = @id AND \"Status\" IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            var p1 = new NpgsqlParameter("time", NpgsqlDbType.Timestamp) { Value = time };
            var p2 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId };

            return ExecuteCommand(connection, command, p1, p2);
        }

        public static int UpdateNextTime(NpgsqlConnection connection, string runtimeId, string nextTimeColumnName, DateTime time,
            NpgsqlTransaction transaction = null)
        {
            var command = String.Format("UPDATE {0} SET \"{1}\" = @time WHERE \"RuntimeId\" = @id", ObjectName, nextTimeColumnName);
            var p1 = new NpgsqlParameter("time", NpgsqlDbType.Timestamp) { Value = time };
            var p2 = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId };

            return ExecuteCommand(connection, command, transaction, p1, p2);
        }

        public static DateTime? GetMaxNextTime(NpgsqlConnection connection, string runtimeId, string nextTimeColumnName)
        {
            var commandText = String.Format("SELECT MAX(\"{1}\") FROM {0} WHERE \"Status\" = 0 AND \"RuntimeId\" != @id", ObjectName, nextTimeColumnName);

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new NpgsqlCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = runtimeId });

            object result = command.ExecuteScalar();

            if (result == null)
            {
                return null;
            }

            return result as DateTime?;
        }
    }
}
