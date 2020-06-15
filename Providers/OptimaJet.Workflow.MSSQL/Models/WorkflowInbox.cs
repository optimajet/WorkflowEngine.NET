using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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

        public static int DeleteByProcessId(SqlConnection connection, Guid processId,
            SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };
            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE [ProcessId] = @processid", ObjectName), transaction, pProcessId);
        }

        public static WorkflowInbox[] SelectByProcessId(SqlConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE [ProcessId] = @processid", ObjectName);

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };

            return Select(connection, selectText, p1);
        }
    }
}
