using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.SQLite.Models
{
    public class WorkflowSync : DbObject<SyncEntity>
    {
        public WorkflowSync(string schemaName, int commandTimeout) : base(schemaName, "WorkflowSync", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SyncEntity.Name), IsKey = true},
                new ColumnInfo {Name = nameof(SyncEntity.Lock), Type = DbType.Guid}
            });
        }

        public async Task<SyncEntity> GetByNameAsync(SqliteConnection connection, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(SyncEntity.Name)} = @name";
            
            SyncEntity[] locks = await SelectAsync(connection, selectText, new SqliteParameter("name", DbType.String) { Value = name }).ConfigureAwait(false);

            return locks.FirstOrDefault();
        }

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
