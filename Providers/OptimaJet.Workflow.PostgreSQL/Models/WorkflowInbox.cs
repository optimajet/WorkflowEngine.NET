using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace OptimaJet.Workflow.PostgreSQL
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
                new ColumnInfo {Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="ProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="IdentityId"}
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
                    Id = (Guid)value;
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

        public static async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId,
            NpgsqlTransaction transaction = null)
        {
            var pProcessId = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };
            return await ExecuteCommandAsync(connection,
                    $"DELETE FROM {ObjectName} WHERE \"ProcessId\" = @processid", transaction, pProcessId)
                .ConfigureAwait(false);
        }

        public static async Task<WorkflowInbox[]> SelectByProcessIdAsync(NpgsqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"ProcessId\" = @processid";

            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
