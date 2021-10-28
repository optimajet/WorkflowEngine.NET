using System;
using System.Collections.Generic;
using System.Data;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        static WorkflowProcessInstanceStatus()
        {
            DbTableName = "WorkflowProcessInstanceStatus";
        }


        public WorkflowProcessInstanceStatus()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "Lock", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "Status", Type = SqlDbType.TinyInt},
                new ColumnInfo {Name = "RuntimeId", Type = SqlDbType.NVarChar},
                new ColumnInfo {Name = "SetTime", Type = SqlDbType.DateTime}
            });
        }

        public Guid Id { get; set; }
        public Guid Lock { get; set; }
        public byte Status { get; set; }

        public string RuntimeId { get; set; }

        public DateTime SetTime { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "Lock":
                    return Lock;
                case "Status":
                    return Status;
                case "RuntimeId":
                    return RuntimeId;
                case "SetTime":
                    return SetTime;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = (Guid) value;
                    break;
                case "Lock":
                    Lock = (Guid) value;
                    break;
                case "Status":
                    Status = (byte) value;
                    break;
                case "RuntimeId":
                    RuntimeId = (string)value;
                    break;
                case "SetTime":
                    SetTime = (DateTime)value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<List<Guid>> GetProcessesByStatusAsync(SqlConnection connection, byte status, string runtimeId)
        {
            string command = $"SELECT [Id] FROM {ObjectName} WHERE [Status] = @status";
            var p = new List<SqlParameter>(); 

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += " AND [RuntimeId] = @runtime";
                p.Add(new SqlParameter("runtime", SqlDbType.NVarChar) { Value = runtimeId });
            }

            p.Add(new SqlParameter("status", SqlDbType.TinyInt) { Value = status });
            
            return (await SelectAsync(connection, command, p.ToArray()).ConfigureAwait(false)).Select(s => s.Id).ToList();
        }

        public static async Task<int> ChangeStatusAsync(SqlConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            string command = $"UPDATE {ObjectName} SET [Status] = @newstatus, [Lock] = @newlock, [SetTime] = @settime, [RuntimeId] = @runtimeid WHERE [Id] = @id AND [Lock] = @oldlock";
            var p1 = new SqlParameter("newstatus", SqlDbType.TinyInt) {Value = status.Status};
            var p2 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) {Value = status.Lock};
            var p3 = new SqlParameter("id", SqlDbType.UniqueIdentifier) {Value = status.Id};
            var p4 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) {Value = oldLock};
            var p5 = new SqlParameter("settime", SqlDbType.DateTime) { Value = status.SetTime };
            var p6 = new SqlParameter("runtimeid", SqlDbType.NVarChar) { Value = status.RuntimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4, p5, p6).ConfigureAwait(false);
        }
#if !NETCOREAPP || NETCORE2
        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(Guid));
            dt.Columns.Add("Lock", typeof(Guid));
            dt.Columns.Add("Status", typeof(byte));
            dt.Columns.Add("RuntimeId", typeof(string));
            dt.Columns.Add("SetTime", typeof(DateTime));
            return dt;
        }
#endif
    }
}
