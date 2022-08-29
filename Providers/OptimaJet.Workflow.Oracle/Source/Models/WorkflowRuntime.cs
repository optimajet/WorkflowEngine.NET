using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Model;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle.Models
{
    public class WorkflowRuntime : DbObject<RuntimeEntity>
    {
        public WorkflowRuntime(string schemaName, int commandTimeout) : base(schemaName, "WorkflowRuntime", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(RuntimeEntity.RuntimeId), IsKey = true, Type = OracleDbType.NVarchar2, Size = 450},
                new ColumnInfo {Name = "LOCKFLAG", Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(RuntimeEntity.Status), Type = OracleDbType.Int16},
                new ColumnInfo {Name = nameof(RuntimeEntity.RestorerId), Type = OracleDbType.NVarchar2, Size = 1024},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextTimerTime), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextServiceTimerTime), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(RuntimeEntity.LastAliveSignal), Type = OracleDbType.TimeStamp}
            });
        }

        public async Task<int> UpdateStatusAsync(OracleConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"{nameof(RuntimeEntity.Status).ToUpperInvariant()} = :newstatus, " + 
                             $"LOCKFLAG = :newlock " + 
                             $"WHERE {nameof(RuntimeEntity.RuntimeId).ToUpperInvariant()} = :id " + 
                             $"AND LOCKFLAG = :oldlock";
            
            var p1 = new OracleParameter("newstatus", OracleDbType.Int16, runtime.Status, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, runtime.Lock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.NVarchar2, runtime.RuntimeId, ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<int> UpdateRestorerAsync(OracleConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"{nameof(RuntimeEntity.RestorerId).ToUpperInvariant()} = :restorer, " + 
                             $"LOCKFLAG = :newlock " + 
                             $"WHERE {nameof(RuntimeEntity.RuntimeId).ToUpperInvariant()} = :id " + 
                             $"AND LOCKFLAG = :oldlock";
            
            var p1 = new OracleParameter("restorer", OracleDbType.NVarchar2, runtime.RestorerId, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, runtime.Lock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.NVarchar2, runtime.RuntimeId, ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<bool> MultiServerRuntimesExistAsync(OracleConnection connection)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(RuntimeEntity.RuntimeId).ToUpperInvariant()} != :empty " + 
                                $"AND {nameof(RuntimeEntity.Status).ToUpperInvariant()} " + 
                                $"NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated})";
            
            RuntimeEntity[] runtimes = await SelectAsync(connection, selectText, new OracleParameter("empty", OracleDbType.NVarchar2, Guid.Empty.ToString(), ParameterDirection.Input)).ConfigureAwait(false);

            return runtimes.Length > 0;
        }

        public async Task<int> ActiveMultiServerRuntimesCountAsync(OracleConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(RuntimeEntity.RuntimeId).ToUpperInvariant()} != :currentruntime " + 
                                $"AND {nameof(RuntimeEntity.Status).ToUpperInvariant()} " + 
                                $"IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            
            RuntimeEntity[] runtimes = await SelectAsync(connection, selectText, new OracleParameter("currentruntime", OracleDbType.NVarchar2, currentRuntimeId, ParameterDirection.Input)).ConfigureAwait(false);

            return runtimes.Length;
        }

        public async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(OracleConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(RuntimeEntity.RuntimeId).ToUpperInvariant()} = :id";
            
            RuntimeEntity r = (await SelectAsync(connection, selectText, new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input)).ConfigureAwait(false))
                .FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.Lock, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime };
        }

        public async Task<int> SendRuntimeLastAliveSignalAsync(OracleConnection connection, string runtimeId, DateTime time, 
            OracleTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET {nameof(RuntimeEntity.LastAliveSignal).ToUpperInvariant()} = :time " + 
                             $"WHERE {nameof(RuntimeEntity.RuntimeId).ToUpperInvariant()} = :id " + 
                             $"AND {nameof(RuntimeEntity.Status).ToUpperInvariant()} " + 
                             $"IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            
            var p1 = new OracleParameter("time", OracleDbType.TimeStamp, time, ParameterDirection.Input);
            var p2 = new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2).ConfigureAwait(false);
        }

        public async Task<int> UpdateNextTimeAsync(OracleConnection connection, string runtimeId, string nextTimeColumnName, DateTime time, 
            OracleTransaction transaction = null)
        {
            string command = $"UPDATE {DbTableName} SET {nextTimeColumnName} = :time " + 
                             $"WHERE {nameof(RuntimeEntity.RuntimeId).ToUpperInvariant()} = :id";
            
            var p1 = new OracleParameter("time", OracleDbType.TimeStamp, time, ParameterDirection.Input);
            var p2 = new OracleParameter("id", OracleDbType.NVarchar2, runtimeId, ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2).ConfigureAwait(false);
        }

        public async Task<DateTime?> GetMaxNextTimeAsync(OracleConnection connection, string runtimeId, string nextTimeColumnName)
        {
            string commandText = $"SELECT MAX({nextTimeColumnName}) FROM {DbTableName} " + 
                                 $"WHERE {nameof(RuntimeEntity.Status).ToUpperInvariant()} = 0 " + 
                                 $"AND {nameof(RuntimeEntity.RuntimeId).ToUpperInvariant()} != :id";

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
