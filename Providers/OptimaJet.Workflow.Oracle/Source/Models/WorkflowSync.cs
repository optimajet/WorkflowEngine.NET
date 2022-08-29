using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle.Models
{
    public class WorkflowSync : DbObject<SyncEntity>
    {
        public WorkflowSync(string schemaName, int commandTimeout) : base(schemaName, "WorkflowSync", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SyncEntity.Name), IsKey = true, Type = OracleDbType.NVarchar2, Size = 450},
                new ColumnInfo {Name = "LOCKFLAG", Type = OracleDbType.Raw}
            });
        }

        public async Task<SyncEntity> GetByNameAsync(OracleConnection connection, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE {nameof(SyncEntity.Name).ToUpperInvariant()} = :name";
            
            SyncEntity[] locks = await SelectAsync(connection, selectText, new OracleParameter("name", OracleDbType.NVarchar2, name, ParameterDirection.Input)).ConfigureAwait(false);

            return locks.FirstOrDefault();
        }

        public async Task<int> UpdateLockAsync(OracleConnection connection, string name, Guid oldLock, Guid newLock, 
            OracleTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET " + 
                             $"LOCKFLAG = :newlock " + 
                             $"WHERE {nameof(SyncEntity.Name).ToUpperInvariant()} = :name " + 
                             $"AND LOCKFLAG = :oldlock";
            
            var p1 = new OracleParameter("newlock", OracleDbType.Raw, newLock.ToByteArray(), ParameterDirection.Input);
            var p2 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("name", OracleDbType.NVarchar2, name, ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2, p3).ConfigureAwait(false);
        }
    }
}
