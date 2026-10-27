using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    /// <summary>
    /// Database object for workflow process instance status operations.
    /// </summary>
    public class WorkflowProcessInstanceStatus : DbObject<ProcessInstanceStatusEntity>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WorkflowProcessInstanceStatus"/>.
        /// </summary>
        /// <param name="schemaName">The database schema name.</param>
        /// <param name="commandTimeout">The command timeout in seconds.</param>
        public WorkflowProcessInstanceStatus(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstanceStatus", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Lock), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.Status), Type = DbType.Byte},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.RuntimeId)},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.SetTime), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessInstanceStatusEntity.TenantId)}
            });
        }

        /// <summary>
        /// Returns identifiers of processes with the specified status, optionally filtered by runtime.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="status">The process status byte value.</param>
        /// <param name="runtimeId">The runtime identifier to filter by, or <c>null</c> for all runtimes.</param>
        /// <returns>A list of process identifiers.</returns>
        public async Task<List<Guid>> GetProcessesByStatusAsync(SqliteConnection connection, byte status, string runtimeId = null)
        {
            string command = $"SELECT {nameof(ProcessInstanceStatusEntity.Id)} " + 
                             $"FROM {ObjectName} WHERE {nameof(ProcessInstanceStatusEntity.Status)} = @status";

            var p = new List<SqliteParameter>();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += $" AND {nameof(ProcessInstanceStatusEntity.RuntimeId)} = @runtime";
                p.Add(new SqliteParameter("runtime", DbType.String) { Value = runtimeId });
            }

            p.Add(new SqliteParameter("status", DbType.Byte) { Value = status });
            return (await SelectAsync(connection, command, p.ToArray()).ConfigureAwait(false)).Select(s => s.Id).ToList();
        }

        /// <summary>
        /// Changes the status of a process instance using optimistic locking.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="status">The status entity with the new status values.</param>
        /// <param name="oldLock">The current lock value for optimistic concurrency.</param>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> ChangeStatusAsync(SqliteConnection connection, ProcessInstanceStatusEntity status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"{nameof(ProcessInstanceStatusEntity.Status)} = @newstatus, " + 
                             $"{nameof(ProcessInstanceStatusEntity.Lock)} = @newlock," +
                             $"{nameof(ProcessInstanceStatusEntity.SetTime)} = @settime, " + 
                             $"{nameof(ProcessInstanceStatusEntity.RuntimeId)} = @runtimeid " +
                             $"WHERE {nameof(ProcessInstanceStatusEntity.Id)} = @id " + 
                             $"AND {nameof(ProcessInstanceStatusEntity.Lock)} = @oldlock";
            
            var p1 = new SqliteParameter("newstatus", DbType.Byte) { Value = status.Status };
            var p2 = new SqliteParameter("newlock", DbType.String) { Value = ToDbValue(status.Lock, DbType.Guid) };
            var p3 = new SqliteParameter("id", DbType.String) { Value = ToDbValue(status.Id, DbType.Guid) };
            var p4 = new SqliteParameter("oldlock", DbType.String) { Value = ToDbValue(oldLock, DbType.Guid) };
            var p5 = new SqliteParameter("settime", DbType.Int64) { Value = ToDbValue(status.SetTime, DbType.DateTime2) };
            var p6 = new SqliteParameter("runtimeid", DbType.String) { Value = status.RuntimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4, p5, p6).ConfigureAwait(false);
        }
    }
}
