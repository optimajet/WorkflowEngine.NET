using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.DbPersistence;
using OptimaJet.Workflow.Core.Entities;

namespace OptimaJet.Workflow.MSSQL.Models
{
    public class WorkflowSync : DbObject<SyncEntity>
    {
        public WorkflowSync(string schemaName, int commandTimeout) : base(schemaName, "WorkflowSync", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SyncEntity.Name), IsKey = true, Type = SqlDbType.NVarChar, Size = 450},
                new ColumnInfo {Name = nameof(SyncEntity.Lock), Type = SqlDbType.UniqueIdentifier}
            });
        }

        public async Task<SyncEntity> GetByNameAsync(SqlConnection connection, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [{nameof(SyncEntity.Name)}] = @name";
            var locks = await SelectAsync(connection, selectText, new SqlParameter("name", SqlDbType.NVarChar) { Value = name }).ConfigureAwait(false);

            return locks.FirstOrDefault();
        }

        public async Task<int> UpdateLockAsync(SqlConnection connection, string name, Guid newLock, Guid oldLock, SqlTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"[{nameof(SyncEntity.Lock)}] = @newlock " + 
                             $"WHERE [{nameof(SyncEntity.Name)}] = @name " + 
                             $"AND [{nameof(SyncEntity.Lock)}] = @oldlock";
            
            var p1 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) { Value = newLock };
            var p2 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) { Value = oldLock };
            var p3 = new SqlParameter("name", SqlDbType.NVarChar) { Value = name };

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2, p3).ConfigureAwait(false);
        }
    }
}
