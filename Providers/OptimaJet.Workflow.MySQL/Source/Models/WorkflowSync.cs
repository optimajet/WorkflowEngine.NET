using System;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.MySQL.Models
{
    public class WorkflowSync : DbObject<SyncEntity>
    {
        public WorkflowSync(int commandTimeout) : base("workflowsync", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SyncEntity.Name), IsKey = true, Type = MySqlDbType.VarString, Size = 450},
                new ColumnInfo {Name = nameof(SyncEntity.Lock), Type = MySqlDbType.Binary}
            });
        }
        
        public async Task<SyncEntity> GetByNameAsync(MySqlConnection connection, string name)
        {
            string selectText = $"SELECT * FROM {DbTableName} " +
                                $"WHERE `{nameof(SyncEntity.Name)}` = @name";

            SyncEntity[] locks = await SelectAsync(connection, selectText, new MySqlParameter("name", MySqlDbType.VarString) {Value = name})
                .ConfigureAwait(false);

            return locks.FirstOrDefault();
        }

        public async Task<int> UpdateLockAsync(MySqlConnection connection, string name, Guid oldLock, Guid newLock,
            MySqlTransaction transaction = null)
        {
            string command = $"UPDATE {DbTableName} SET " +
                             $"`{nameof(SyncEntity.Lock)}` = @newlock " +
                             $"WHERE `{nameof(SyncEntity.Name)}` = @name " +
                             $"AND `{nameof(SyncEntity.Lock)}` = @oldlock";

            var p1 = new MySqlParameter("newlock", MySqlDbType.Binary) {Value = newLock.ToByteArray()};
            var p2 = new MySqlParameter("oldlock", MySqlDbType.Binary) {Value = oldLock.ToByteArray()};
            var p3 = new MySqlParameter("name", MySqlDbType.VarString) {Value = name};

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2, p3).ConfigureAwait(false);
        }
    }
}
