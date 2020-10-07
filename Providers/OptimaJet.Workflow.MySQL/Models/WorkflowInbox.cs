using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace OptimaJet.Workflow.MySQL
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
                new ColumnInfo {Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name="ProcessId", Type = MySqlDbType.Binary},
                new ColumnInfo {Name="IdentityId", Type = MySqlDbType.String}
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

        public static async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId,
            MySqlTransaction transaction = null)
        {
            var pProcessId = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };
            return await ExecuteCommandAsync(connection,
                $"DELETE FROM {DbTableName} WHERE `ProcessId` = @processid", transaction, pProcessId).ConfigureAwait(false);
        }

        public static async Task<WorkflowInbox[]> SelectByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `ProcessId` = @processid";

            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
