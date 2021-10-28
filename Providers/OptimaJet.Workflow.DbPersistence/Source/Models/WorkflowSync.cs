using System;
using System.Data;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.DbPersistence;

namespace OptimaJet.Workflow.MSSQL.Models
{
    public class WorkflowSync : DbObject<WorkflowSync>
    {
        static WorkflowSync()
        {
            DbTableName = "WorkflowSync";
        }

        public WorkflowSync()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Name", IsKey = true, Type = SqlDbType.NVarChar, Size = 450},
                new ColumnInfo {Name = "Lock", Type = SqlDbType.UniqueIdentifier}
            });
        }

        public string Name { get; set; }
        public Guid Lock { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Name":
                    return Name;
                case "Lock":
                    return Lock;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Lock":
                    Lock = (Guid)value;
                    break;
                case "Name":
                    Name = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<WorkflowSync> GetByNameAsync(SqlConnection connection, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [Name] = @name";
            WorkflowSync[] locks = await SelectAsync(connection, selectText, new SqlParameter("name", SqlDbType.NVarChar) { Value = name }).ConfigureAwait(false);

            return locks.FirstOrDefault();
        }

        public static async Task<int> UpdateLockAsync(SqlConnection connection, string name, Guid oldLock, Guid newLock, SqlTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} SET [Lock] = @newlock WHERE [Name] = @name AND [Lock] = @oldlock";
            var p1 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) { Value = newLock };
            var p2 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) { Value = oldLock };
            var p3 = new SqlParameter("name", SqlDbType.NVarChar) { Value = name };

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, p1, p2, p3).ConfigureAwait(false);
        }
    }
}
