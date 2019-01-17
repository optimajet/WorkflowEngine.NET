using System;
using System.Data;
using System.Data.SqlClient;

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
            DbColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "Lock", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "Status", Type = SqlDbType.TinyInt}
            });
        }

        public Guid Id { get; set; }
        public Guid Lock { get; set; }
        public byte Status { get; set; }

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
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int MassChangeStatus(SqlConnection connection, byte stateFrom, byte stateTo)
        {
            var command = string.Format("UPDATE {0} SET [Status] = @stateto WHERE [Status] = @statefrom", ObjectName);
            var p1 = new SqlParameter("statefrom", SqlDbType.TinyInt) {Value = stateFrom};
            var p2 = new SqlParameter("stateto", SqlDbType.TinyInt) {Value = stateTo};

            return ExecuteCommand(connection, command, p1, p2);
        }

        public static int ChangeStatus(SqlConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            var command = string.Format("UPDATE {0} SET [Status] = @newstatus, [Lock] = @newlock WHERE [Id] = @id AND [Lock] = @oldlock", ObjectName);
            var p1 = new SqlParameter("newstatus", SqlDbType.TinyInt) {Value = status.Status};
            var p2 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) {Value = status.Lock};
            var p3 = new SqlParameter("id", SqlDbType.UniqueIdentifier) {Value = status.Id};
            var p4 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) {Value = oldLock};

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }
#if !NETCOREAPP || NETCORE2
        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(Guid));
            dt.Columns.Add("Lock", typeof(Guid));
            dt.Columns.Add("Status", typeof(byte));
            return dt;
        }
#endif
    }
}