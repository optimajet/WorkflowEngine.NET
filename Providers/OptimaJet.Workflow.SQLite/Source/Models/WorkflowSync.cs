using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.SQLite.Models
{
    /// <summary>
    /// Database object for workflow synchronization lock operations.
    /// </summary>
    public class WorkflowSync : DbObject<SyncEntity>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WorkflowSync"/>.
        /// </summary>
        /// <param name="schemaName">The database schema name.</param>
        /// <param name="commandTimeout">The command timeout in seconds.</param>
        public WorkflowSync(string schemaName, int commandTimeout) : base(schemaName, "WorkflowSync", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SyncEntity.Name), IsKey = true},
                new ColumnInfo {Name = nameof(SyncEntity.Lock), Type = DbType.Guid}
            });
        }

        /// <summary>
        /// Gets a synchronization lock entity by its name.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="name">The lock name.</param>
        /// <returns>The sync entity, or <c>null</c> if not found.</returns>
        public async Task<SyncEntity> GetByNameAsync(SqliteConnection connection, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(SyncEntity.Name)} = @name";
            
            SyncEntity[] locks = await SelectAsync(connection, selectText, new SqliteParameter("name", DbType.String) { Value = name }).ConfigureAwait(false);

            return locks.FirstOrDefault();
        }

        /// <summary>
        /// Updates the synchronization lock value using optimistic concurrency.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="name">The lock name.</param>
        /// <param name="oldLock">The current lock value.</param>
        /// <param name="newLock">The new lock value to set.</param>
        /// <param name="transaction">An optional transaction.</param>
        /// <returns>The number of rows affected (0 if the lock was changed by another process).</returns>
        public async Task<int> UpdateLockAsync(SqliteConnection connection, string name, Guid oldLock, Guid newLock, SqliteTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET Lock = @newlock " + 
                             $"WHERE {nameof(SyncEntity.Name)} = @name " + 
                             $"AND {nameof(SyncEntity.Lock)} = @oldlock";
            
            var p1 = new SqliteParameter("newlock", DbType.String) { Value = ToDbValue(newLock, DbType.Guid) };
            var p2 = new SqliteParameter("oldlock", DbType.String) { Value = ToDbValue(oldLock, DbType.Guid) };
            var p3 = new SqliteParameter("name", DbType.String) { Value = name };

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2, p3).ConfigureAwait(false);
        }

    }
}
