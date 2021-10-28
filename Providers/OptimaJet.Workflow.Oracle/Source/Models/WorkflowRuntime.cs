using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static async Task<int> UpdateStatusAsync(OracleConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET STATUS = :newstatus, LOCKFLAG = :newlock WHERE RUNTIMEID = :id AND LOCKFLAG = :oldlock";
            var p1 = new OracleParameter("newstatus", OracleDbType.Int16, runtime.Status, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, runtime.Lock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.NVarchar2, runtime.RuntimeId, ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public static async Task<int> UpdateRestorerAsync(OracleConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET RESTORERID = :restorer, LOCKFLAG = :newlock WHERE RUNTIMEID = :id AND LOCKFLAG = :oldlock";
            var p1 = new OracleParameter("restorer", OracleDbType.NVarchar2, runtime.RestorerId, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, runtime.Lock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.NVarchar2, runtime.RuntimeId, ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public static async Task<bool> MultiServerRuntimesExistAsync(OracleConnection connection)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE RUNTIMEID != :empty AND STATUS NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated})";
            WorkflowRuntime[] runtimes = await SelectAsync(connection, selectText, new OracleParameter("empty", OracleDbType.NVarchar2, Guid.Empty.ToString(), ParameterDirection.Input)).ConfigureAwait(false);

            return runtimes.Length > 0;
        }

        public static async Task<int> ActiveMultiServerRuntimesCountAsync(OracleConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE RUNTIMEID != :current AND STATUS IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            WorkflowRuntime[] runtimes = await SelectAsync(connection, selectText, new OracleParameter("current", OracleDbType.NVarchar2, currentRuntimeId, ParameterDirection.Input)).ConfigureAwait(false);

            return runtimes.Length;
        }

        public static async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(OracleConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE RUNTIMEID = :id";
            WorkflowRuntime r = (await SelectAsync(connection, selectText, new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input)).ConfigureAwait(false))
                .FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.LOCKFLAG, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime };
        }

        public static async Task<int> SendRuntimeLastAliveSignalAsync(OracleConnection connection, string runtimeId, DateTime time)
        {
            string command = $"UPDATE {ObjectName} SET LASTALIVESIGNAL = :time WHERE RUNTIMEID = :id AND STATUS IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            var p1 = new OracleParameter("time", OracleDbType.TimeStamp, time, ParameterDirection.Input);
            var p2 = new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2).ConfigureAwait(false);
        }

        public static async Task<int> UpdateNextTimeAsync(OracleConnection connection, string runtimeId, string nextTimeColumnName, DateTime time)
        {
            string command = $"UPDATE {DbTableName} SET {nextTimeColumnName} = :time WHERE RUNTIMEID = :id";
            var p1 = new OracleParameter("time", OracleDbType.TimeStamp, time, ParameterDirection.Input);
            var p2 = new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2).ConfigureAwait(false);
        }

        public static async Task<DateTime?> GetMaxNextTimeAsync(OracleConnection connection, string runtimeId, string nextTimeColumnName)
        {
            string commandText = $"SELECT MAX({nextTimeColumnName}) FROM {DbTableName} WHERE STATUS = 0 AND RUNTIMEID != :id";

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new OracleCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input));

            object result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            return result as DateTime?;
        }
    }
}
