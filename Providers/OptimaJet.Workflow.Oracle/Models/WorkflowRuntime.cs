using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle.Models
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
                new ColumnInfo {Name = "RuntimeId", IsKey = true, Type = OracleDbType.NVarchar2, Size = 450},
                new ColumnInfo {Name = "LOCKFLAG", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "Status", Type = OracleDbType.Int16},
                new ColumnInfo {Name = "RestorerId", Type = OracleDbType.NVarchar2, Size = 1024},
                new ColumnInfo {Name = "NextTimerTime", Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = "NextServiceTimerTime", Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = "LastAliveSignal", Type = OracleDbType.TimeStamp}
            });
        }

        public string RuntimeId { get; set; }
        public Guid LOCKFLAG { get; set; }
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
                case "LOCKFLAG":
                    return LOCKFLAG.ToByteArray();
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
                case "LOCKFLAG":
                    LOCKFLAG = new Guid((byte[])value);
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

        public static int UpdateStatus(OracleConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            var command = String.Format("UPDATE {0} SET STATUS = :newstatus, LOCKFLAG = :newlock WHERE RUNTIMEID = :id AND LOCKFLAG = :oldlock", ObjectName);
            var p1 = new OracleParameter("newstatus", OracleDbType.Int16, runtime.Status, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, runtime.Lock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.NVarchar2, runtime.RuntimeId, ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }

        public static int UpdateRestorer(OracleConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            var command = String.Format("UPDATE {0} SET RESTORERID = :restorer, LOCKFLAG = :newlock WHERE RUNTIMEID = :id AND LOCKFLAG = :oldlock", ObjectName);
            var p1 = new OracleParameter("restorer", OracleDbType.NVarchar2, runtime.RestorerId, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, runtime.Lock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.NVarchar2, runtime.RuntimeId, ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }

        public static bool MultiServerRuntimesExist(OracleConnection connection)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE RUNTIMEID != :empty OR STATUS NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated}, {(int)RuntimeStatus.Single})";
            var runtimes = Select(connection, selectText, new OracleParameter("empty", OracleDbType.NVarchar2, Guid.Empty.ToString(), ParameterDirection.Input));

            return runtimes.Length > 0;
        }

        public static int SingleServerRuntimesCount(OracleConnection connection)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE RUNTIMEID = :empty AND STATUS = {(int)RuntimeStatus.Single}";
            var runtimes = Select(connection, selectText, new OracleParameter("empty", OracleDbType.NVarchar2, Guid.Empty.ToString(), ParameterDirection.Input));

            return runtimes.Length;
        }

        public static int ActiveMultiServerRuntimesCount(OracleConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE RUNTIMEID != :current AND STATUS IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            var runtimes = Select(connection, selectText, new OracleParameter("current", OracleDbType.NVarchar2, currentRuntimeId, ParameterDirection.Input));

            return runtimes.Length;
        }

        public static WorkflowRuntimeModel GetWorkflowRuntimeStatus(OracleConnection connection, string runtimeId)
        {
            string selectText = String.Format("SELECT * FROM {0} WHERE RUNTIMEID = :id", ObjectName);
            WorkflowRuntime r = Select(connection, selectText, new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input))
                .FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.LOCKFLAG, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime };
        }

        public static int SendRuntimeLastAliveSignal(OracleConnection connection, string runtimeId, DateTime time)
        {
            var command = $"UPDATE {ObjectName} SET LASTALIVESIGNAL = :time WHERE RUNTIMEID = :id AND STATUS IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            var p1 = new OracleParameter("time", OracleDbType.TimeStamp, time, ParameterDirection.Input);
            var p2 = new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input);

            return ExecuteCommand(connection, command, p1, p2);
        }

        public static int UpdateNextTime(OracleConnection connection, string runtimeId, string nextTimeColumnName, DateTime time)
        {
            var command = String.Format("UPDATE {0} SET {1} = :time WHERE RUNTIMEID = :id", DbTableName, nextTimeColumnName);
            var p1 = new OracleParameter("time", OracleDbType.TimeStamp, time, ParameterDirection.Input);
            var p2 = new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input);

            return ExecuteCommand(connection, command, p1, p2);
        }

        public static DateTime? GetMaxNextTime(OracleConnection connection, string runtimeId, string nextTimeColumnName)
        {
            var commandText = String.Format("SELECT MAX({1}) FROM {0} WHERE STATUS = 0 AND RUNTIMEID != :id", DbTableName, nextTimeColumnName);

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new OracleCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input));

            object result = command.ExecuteScalar();

            if (result == null)
            {
                return null;
            }

            return result as DateTime?;
        }
    }
}
