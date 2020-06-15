using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.DbPersistence;

namespace OptimaJet.Workflow.MSSQL.Models
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
                new ColumnInfo {Name = "RuntimeId", IsKey = true, Type = SqlDbType.NVarChar, Size = 450},
                new ColumnInfo {Name = "Lock", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "Status", Type = SqlDbType.TinyInt},
                new ColumnInfo {Name = "RestorerId", Type = SqlDbType.NVarChar, Size = 1024},
                new ColumnInfo {Name = "NextTimerTime", Type = SqlDbType.DateTime},
                new ColumnInfo {Name = "NextServiceTimerTime", Type = SqlDbType.DateTime},
                new ColumnInfo {Name = "LastAliveSignal", Type = SqlDbType.DateTime}
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
                    return (byte)Status;
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
                    Status = (RuntimeStatus)value;
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

        public static bool MultiServerRuntimesExist(SqlConnection connection)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE [RuntimeId] != @empty OR [Status] NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated}, {(int)RuntimeStatus.Single})";
            var runtimes = Select(connection, selectText, new SqlParameter("empty", SqlDbType.NVarChar) { Value = Guid.Empty.ToString() });

            return runtimes.Length > 0;
        }

        public static int SingleServerRuntimesCount(SqlConnection connection)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE [RuntimeId] = @empty AND [Status] =  {(int)RuntimeStatus.Single}";
            var runtimes = Select(connection, selectText, new SqlParameter("empty", SqlDbType.NVarChar) { Value = Guid.Empty.ToString() });

            return runtimes.Length;
        }

        public static int ActiveMultiServerRuntimesCount(SqlConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [RuntimeId] != @current AND [Status] IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            WorkflowRuntime[] runtimes = Select(connection, selectText, new SqlParameter("current", SqlDbType.NVarChar) { Value = currentRuntimeId });

            return runtimes.Length;
        }

        public static int UpdateStatus(SqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            var command = String.Format("UPDATE {0} SET [Status] = @newstatus, [Lock] = @newlock WHERE [RuntimeId] = @id AND [Lock] = @oldlock", ObjectName);
            var p1 = new SqlParameter("newstatus", SqlDbType.TinyInt) { Value = status.Status };
            var p2 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) { Value = status.Lock };
            var p3 = new SqlParameter("id", SqlDbType.NVarChar) { Value = status.RuntimeId };
            var p4 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) { Value = oldLock };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }

        public static int UpdateRestorer(SqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            var command = String.Format("UPDATE {0} SET [RestorerId] = @restorer, [Lock] = @newlock WHERE [RuntimeId] = @id AND [Lock] = @oldlock", ObjectName);
            var p1 = new SqlParameter("restorer", SqlDbType.NVarChar) { Value = status.RestorerId };
            var p2 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) { Value = status.Lock };
            var p3 = new SqlParameter("id", SqlDbType.NVarChar) { Value = status.RuntimeId };
            var p4 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) { Value = oldLock };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }

        public static WorkflowRuntimeModel GetWorkflowRuntimeStatus(SqlConnection connection, string runtimeId)
        {
            string selectText = String.Format("SELECT * FROM {0} WHERE [RuntimeId] = @id", ObjectName);
            WorkflowRuntime r = Select(connection, selectText, new SqlParameter("id", SqlDbType.NVarChar) { Value = runtimeId }).FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.Lock, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime  };
        }

        public static int SendRuntimeLastAliveSignal(SqlConnection connection, string runtimeId, DateTime time)
        {
            var command = $"UPDATE {ObjectName} SET [LastAliveSignal] = @time WHERE [RuntimeId] = @id AND [Status] IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            var p1 = new SqlParameter("time", SqlDbType.DateTime) { Value = time };
            var p2 = new SqlParameter("id", SqlDbType.NVarChar) { Value = runtimeId };
           
            return ExecuteCommand(connection, command, p1, p2);
        }

        public static int UpdateNextTime(SqlConnection connection, string runtimeId, string nextTimeColumnName, DateTime time, 
            SqlTransaction transaction = null)
        {
            var command = String.Format("UPDATE {0} SET [{1}] = @time WHERE [RuntimeId] = @id", ObjectName, nextTimeColumnName);
            var p1 = new SqlParameter("time", SqlDbType.DateTime) { Value = time };
            var p2 = new SqlParameter("id", SqlDbType.NVarChar) { Value = runtimeId };

            return ExecuteCommand(connection, command, transaction, p1, p2);
        }

        public static DateTime? GetMaxNextTime(SqlConnection connection, string runtimeId, string nextTimeColumnName)
        {
            var commandText = $"SELECT MAX([{nextTimeColumnName}]) FROM {ObjectName} WHERE [Status] = {(int)RuntimeStatus.Alive} AND [RuntimeId] != @id";
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new SqlCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new SqlParameter("id", SqlDbType.NVarChar) { Value = runtimeId });

            object result = command.ExecuteScalar();

            return result as DateTime?;
        }
    }
}
