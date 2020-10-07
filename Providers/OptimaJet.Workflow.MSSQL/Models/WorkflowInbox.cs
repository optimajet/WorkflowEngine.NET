using System;
using System.Data;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Threading.Tasks;

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowInbox : DbObject<WorkflowInbox>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }

        static WorkflowInbox()
        {
            DbTableName = "WorkflowInbox";
        }

        public WorkflowInbox()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name="ProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name="IdentityId", Type = SqlDbType.NVarChar}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "ProcessId":
                    return ProcessId;
                case "IdentityId":
                    return IdentityId;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id =  (Guid)value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid)value;
                    break;
                case "IdentityId":
                    IdentityId = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId,
            SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };
            return await ExecuteCommandAsync(connection, $"DELETE FROM {ObjectName} WHERE [ProcessId] = @processid", transaction, pProcessId).ConfigureAwait(false);
        }

        public static async Task<WorkflowInbox[]> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [ProcessId] = @processid";

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
