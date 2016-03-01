using System;
using System.Data.SqlClient;
using System.Data;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        public Guid Id { get; set; }
        public Guid Lock { get; set; }
        public byte Status { get; set; }

        private const string TableName = "WorkflowProcessInstanceStatus";

        public WorkflowProcessInstanceStatus()
        {
            DbTableName = TableName;
            DbColumns.AddRange(new[]{
                new ColumnInfo(){Name="Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo(){Name="Lock", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo(){Name="Status", Type = SqlDbType.TinyInt}
            });
        }

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
                    Id = (Guid)value;
                    break;
                case "Lock":
                    Lock = (Guid)value;
                    break;
                case "Status":
                    Status = (byte)value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int MassChangeStatus(SqlConnection connection, byte stateFrom, byte stateTo)
        {
            string command = string.Format("UPDATE [{0}] SET [Status] = @stateto WHERE [Status] = @statefrom", TableName);
            var p1 = new SqlParameter("statefrom", SqlDbType.TinyInt) {Value = stateFrom};
            var p2 = new SqlParameter("stateto", SqlDbType.TinyInt) {Value = stateTo};

            return ExecuteCommand(connection, command, p1, p2);
        }

        public static int ChangeStatus(SqlConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            string command = string.Format("UPDATE [{0}] SET [Status] = @newstatus, [Lock] = @newlock WHERE [Id] = @id AND [Lock] = @oldlock", TableName);
            var p1 = new SqlParameter("newstatus", SqlDbType.TinyInt) { Value = status.Status };
            var p2 = new SqlParameter("newlock", SqlDbType.UniqueIdentifier) { Value = status.Lock };
            var p3 = new SqlParameter("id", SqlDbType.UniqueIdentifier) { Value = status.Id };
            var p4 = new SqlParameter("oldlock", SqlDbType.UniqueIdentifier) { Value = oldLock };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }
    }
}
