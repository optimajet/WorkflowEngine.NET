using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.MySQL.Models
{
    public class WorkflowRuntime : DbObject<RuntimeEntity>
    {
        public WorkflowRuntime(int commandTimeout) : base("workflowruntime", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(RuntimeEntity.RuntimeId), IsKey = true, Type = MySqlDbType.VarString, Size = 450},
                new ColumnInfo {Name = nameof(RuntimeEntity.Lock), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(RuntimeEntity.Status), Type = MySqlDbType.Byte},
                new ColumnInfo {Name = nameof(RuntimeEntity.RestorerId), Type = MySqlDbType.VarString, Size = 1024},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextTimerTime), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(RuntimeEntity.NextServiceTimerTime), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(RuntimeEntity.LastAliveSignal), Type = MySqlDbType.DateTime}
            });
        }
        
        public async Task<int> UpdateStatusAsync(MySqlConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = $"UPDATE {DbTableName} SET " + 
                             $"`{nameof(RuntimeEntity.Status)}` = @newstatus, " + 
                             $"`{nameof(RuntimeEntity.Lock)}` = @newlock " + 
                             $"WHERE `{nameof(RuntimeEntity.RuntimeId)}` = @id " + 
                             $"AND `{nameof(RuntimeEntity.Lock)}` = @oldlock";
            
            var p1 = new MySqlParameter("newstatus", MySqlDbType.Byte)
            {
                Value = ToDbValue(runtime.Status, MySqlDbType.Byte)
            };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = runtime.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtime.RuntimeId };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<int> UpdateRestorerAsync(MySqlConnection connection, WorkflowRuntimeModel runtime, Guid oldLock)
        {
            string command = $"UPDATE {DbTableName} SET " + 
                             $"`{nameof(RuntimeEntity.RestorerId)}` = @restorer, " + 
                             $"`{nameof(RuntimeEntity.Lock)}` = @newlock " + 
                             $"WHERE `{nameof(RuntimeEntity.RuntimeId)}` = @id " + 
                             $"AND `{nameof(RuntimeEntity.Lock)}` = @oldlock";
            
            var p1 = new MySqlParameter("restorer", MySqlDbType.VarString) { Value = runtime.RestorerId };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = runtime.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtime.RuntimeId };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4).ConfigureAwait(false);
        }

        public async Task<bool> MultiServerRuntimesExistAsync(MySqlConnection connection)
        {
            string selectText = $"SELECT * FROM {DbTableName} " + 
                                $"WHERE `{nameof(RuntimeEntity.RuntimeId)}` != @empty " + 
                                $"AND `{nameof(RuntimeEntity.Status)}` " + 
                                $"NOT IN ({(int)RuntimeStatus.Dead}, {(int)RuntimeStatus.Terminated})";
            
            RuntimeEntity[] runtimes = await SelectAsync(connection, selectText, new MySqlParameter("empty", MySqlDbType.VarString) { Value = Guid.Empty.ToString() }).ConfigureAwait(false);

            return runtimes.Length > 0;
        }

        public async Task<int> GetActiveMultiServerRuntimesCountAsync(MySqlConnection connection, string currentRuntimeId)
        {
            string selectText = $"SELECT * FROM {DbTableName} " + 
                                $"WHERE `{nameof(RuntimeEntity.RuntimeId)}` != @current " + 
                                $"AND `{nameof(RuntimeEntity.Status)}` " + 
                                $"IN ({(int)RuntimeStatus.Alive}, {(int)RuntimeStatus.Restore}, {(int)RuntimeStatus.SelfRestore})";
            
            RuntimeEntity[] runtimes = await SelectAsync(connection, selectText, new MySqlParameter("current", MySqlDbType.VarString) { Value = currentRuntimeId }).ConfigureAwait(false);

            return runtimes.Length;
        }

        public async Task<WorkflowRuntimeModel> GetWorkflowRuntimeStatusAsync(MySqlConnection connection, string runtimeId)
        {
            string selectText = $"SELECT * FROM {DbTableName} " + 
                                $"WHERE `{nameof(RuntimeEntity.RuntimeId)}` = @id";
            
            RuntimeEntity r = (await SelectAsync(connection, selectText, new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId }).ConfigureAwait(false)).FirstOrDefault();

            if (r == null)
            {
                return null;
            }

            return new WorkflowRuntimeModel { Lock = r.Lock, RuntimeId = r.RuntimeId, Status = r.Status, RestorerId = r.RestorerId, LastAliveSignal = r.LastAliveSignal, NextTimerTime = r.NextTimerTime };
        }

        public async Task<int> SendRuntimeLastAliveSignalAsync(MySqlConnection connection, string runtimeId, DateTime time)
        {
            string command = $"UPDATE {DbTableName} SET " + 
                             $"`{nameof(RuntimeEntity.LastAliveSignal)}` = @time " + 
                             $"WHERE `{nameof(RuntimeEntity.RuntimeId)}` = @id " + 
                             $"AND `{nameof(RuntimeEntity.Status)}` " + 
                             $"IN ({(int)RuntimeStatus.Alive},{(int)RuntimeStatus.SelfRestore})";
            
            var p1 = new MySqlParameter("time", MySqlDbType.DateTime) { Value = time };
            var p2 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2).ConfigureAwait(false);
        }

        public async Task<int> UpdateNextTimeAsync(MySqlConnection connection, string runtimeId, string nextTimeColumnName, DateTime time,
            MySqlTransaction transaction = null)
        {
            string command = $"UPDATE {DbTableName} SET " + 
                             $"`{nextTimeColumnName}` = @time " + 
                             $"WHERE `{nameof(RuntimeEntity.RuntimeId)}` = @id";
            
            var p1 = new MySqlParameter("time", MySqlDbType.DateTime) { Value = time };
            var p2 = new MySqlParameter("id", MySqlDbType.VarString) { Value = runtimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2).ConfigureAwait(false);
        }

        public async Task<DateTime?> GetMaxNextTimeAsync(MySqlConnection connection, string runtimeId, string nextTimeColumnName)
        {
            string commandText = $"SELECT MAX(`{nextTimeColumnName}`) FROM {DbTableName} " + 
                                 $"WHERE `{nameof(RuntimeEntity.Status)}` = 0 " + 
                                 $"AND `{nameof(RuntimeEntity.RuntimeId)}` != @id";

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
