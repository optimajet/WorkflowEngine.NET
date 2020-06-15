using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle
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
                new ColumnInfo {Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name="ProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name="IdentityId", Type = OracleDbType.NChar}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "ProcessId":
                    return ProcessId.ToByteArray();
                case "IdentityId":
                    return IdentityId as string;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = new Guid((byte[])value);
                    break;
                case "ProcessId":
                    ProcessId = new Guid((byte[])value);
                    break;
                case "IdentityId":
                    IdentityId = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int DeleteByProcessId(OracleConnection connection, Guid processId)
        {
            string name = "ProcessId";

            var pProcessId = new OracleParameter("processid", OracleDbType.Raw) { Value = processId.ToByteArray() };
            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE {1} = :processid", ObjectName, name.ToUpperInvariant()),pProcessId);
        }

        public static WorkflowInbox[] SelectByProcessId(OracleConnection connection, Guid processId)
        {
            string name = "ProcessId";

            var selectText = string.Format("SELECT * FROM {0} WHERE {1} = :processid", ObjectName, name.ToUpperInvariant());

            var p1 = new OracleParameter("processid", OracleDbType.Raw) { Value = processId.ToByteArray() };

            return Select(connection, selectText, p1);
        }
    }
}
