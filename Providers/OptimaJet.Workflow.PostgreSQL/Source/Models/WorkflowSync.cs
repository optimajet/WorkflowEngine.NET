using System;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.PostgreSQL.Models
{
    public class WorkflowSync : DbObject<SyncEntity>
    {
        public WorkflowSync(string schemaName, int commandTimeout) : base(schemaName, "WorkflowSync", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SyncEntity.Name), IsKey = true},
                new ColumnInfo {Name = nameof(SyncEntity.Lock), Type = NpgsqlDbType.Uuid}
            });
        }

        public async Task<SyncEntity> GetByNameAsync(NpgsqlConnection connection, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE \"{nameof(SyncEntity.Name)}\" = @name";
            
            SyncEntity[] locks = await SelectAsync(connection, selectText, new NpgsqlParameter("name", NpgsqlDbType.Varchar) { Value = name }).ConfigureAwait(false);

            return locks.FirstOrDefault();
        }

        public async Task<int> UpdateLockAsync(NpgsqlConnection connection, string name, Guid oldLock, Guid newLock, NpgsqlTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET \"Lock\" = @newlock " + 
                             $"WHERE \"{nameof(SyncEntity.Name)}\" = @name " + 
                             $"AND \"{nameof(SyncEntity.Lock)}\" = @oldlock";
            
            var p1 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = newLock };
            var p2 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };
            var p3 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) { Value = name };

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2, p3).ConfigureAwait(false);
        }

    }
}
