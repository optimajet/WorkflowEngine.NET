using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.DbPersistence;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.MSSQL.Models
{
    public class WorkflowRuntime : DbObject<RuntimeEntity>
    {
        public WorkflowRuntime(string schemaName, int commandTimeout) : base(schemaName, "WorkflowRuntime", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(RuntimeEntity.RuntimeId), IsKey = true, Type = SqlDbType.NVarChar, Size = 450},
                new ColumnInfo {Name = nameof(RuntimeEntity.Lock), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(RuntimeEntity.Status), Type = SqlDbType.TinyInt},
                new ColumnInfo {Name = nameof(RuntimeEntity.RestorerId), Type = SqlDbType.NVarChar, Size = 1024},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextTimerTime), Type = SqlDbType.DateTime},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextServiceTimerTime), Type = SqlDbType.DateTime},
                new ColumnInfo {Name = nameof(RuntimeEntity.LastAliveSignal), Type = SqlDbType.DateTime}
            });
        }

        public async Task<bool> MultiServerRuntimesExistAsync(SqlConnection connection)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE [{nameof(RuntimeEntity.RuntimeId)}] != @empty " + 
                                $"AND [{nameof(RuntimeEntity.Status)}] " + 
                                $"NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated})";
            
            var runtimes = await SelectAsync(connection, selectText, new SqlParameter("empty", SqlDbType.NVarChar) {Value = Guid.Empty.ToString()}).ConfigureAwait(false);

            return runtimes.Length > 0;
        }

        public async Task<int> GetActiveMultiServerRuntimesCountAsync(SqlConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE [{nameof(RuntimeEntity.RuntimeId)}] != @current " + 
                                $"AND [{nameof(RuntimeEntity.Status)}] " + 
                                $"IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            
            var runtimes = await SelectAsync(connection, selectText, new SqlParameter("current", SqlDbType.NVarChar) { Value = currentRuntimeId }).ConfigureAwait(false);

            return runtimes.Length;
        }

        public async Task<int> UpdateStatusAsync(SqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"[{nameof(RuntimeEntity.Status)}] = @newstatus, " + 
                             $"[{nameof(RuntimeEntity.Lock)}] = @newlock " + 
                             $"WHERE [{nameof(RuntimeEntity.RuntimeId)}] = @id " + 
                             $"AND [{nameof(RuntimeEntity.Lock)}] = @oldlock";
            
            var p1 = new SqlParameter("newstatus", SqlDbType.TinyInt) { Value = status.Status };
            var p2 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) { Value = status.Lock };
            var p3 = new SqlParameter("id", SqlDbType.NVarChar) { Value = status.RuntimeId };
            var p4 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) { Value = oldLock };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<int> UpdateRestorerAsync(SqlConnection connection, WorkflowRuntimeModel status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"[{nameof(RuntimeEntity.RestorerId)}] = @restorer, " + 
                             $"[{nameof(RuntimeEntity.Lock)}] = @newlock " + 
                             $"WHERE [{nameof(RuntimeEntity.RuntimeId)}] = @id " + 
                             $"AND [{nameof(RuntimeEntity.Lock)}] = @oldlock";
            
            var p1 = new SqlParameter("restorer", SqlDbType.NVarChar) { Value = status.RestorerId };
            var p2 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) { Value = status.Lock };
            var p3 = new SqlParameter("id", SqlDbType.NVarChar) { Value = status.RuntimeId };
            var p4 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) { Value = oldLock };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(SqlConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE [{nameof(RuntimeEntity.RuntimeId)}] = @id";

            var r = (await SelectAsync(connection, selectText, new SqlParameter("id", SqlDbType.NVarChar) {Value = runtimeId})
                .ConfigureAwait(false)).FirstOrDefault();

            return r == null
                ? null
                : new WorkflowRuntimeModel
                {
                    Lock = r.Lock,
                    RuntimeId = r.RuntimeId,
                    Status = r.Status,
                    RestorerId = r.RestorerId,
                    LastAliveSignal = r.LastAliveSignal,
                    NextTimerTime = r.NextTimerTime
                };
        }

        public async Task<int> SendRuntimeLastAliveSignalAsync(SqlConnection connection, string runtimeId, DateTime time)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"[{nameof(RuntimeEntity.LastAliveSignal)}] = @time " + 
                             $"WHERE [{nameof(RuntimeEntity.RuntimeId)}] = @id " + 
                             $"AND [{nameof(RuntimeEntity.Status)}] IN (" +
                             (int)RuntimeStatus.Alive + 
                             $",{(int)RuntimeStatus.SelfRestore})";
            
            var p1 = new SqlParameter("time", SqlDbType.DateTime) { Value = time };
            var p2 = new SqlParameter("id", SqlDbType.NVarChar) { Value = runtimeId };
           
            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2).ConfigureAwait(false);
        }

        public async Task<int> UpdateNextTimeAsync(SqlConnection connection, string runtimeId, string nextTimeColumnName, DateTime time, 
            SqlTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"[{nextTimeColumnName}] = @time " + 
                             $"WHERE [{nameof(RuntimeEntity.RuntimeId)}] = @id";
            var p1 = new SqlParameter("time", SqlDbType.DateTime) { Value = time };
            var p2 = new SqlParameter("id", SqlDbType.NVarChar) { Value = runtimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2).ConfigureAwait(false);
        }

        public async Task<DateTime?> GetMaxNextTimeAsync(SqlConnection connection, string runtimeId, string nextTimeColumnName)
        {
            string commandText = $"SELECT MAX([{nextTimeColumnName}]) " + 
                                 $"FROM {ObjectName} WHERE [{nameof(RuntimeEntity.Status)}] = {(int)RuntimeStatus.Alive} " +
                                 $"AND [{nameof(RuntimeEntity.RuntimeId)}] != @id";
            
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
            
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new SqlCommand(commandText, connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            
            command.Parameters.Add(new SqlParameter("id", SqlDbType.NVarChar) { Value = runtimeId });

            object result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            return result as DateTime?;
        }
    }
}
