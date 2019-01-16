using System;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        public Guid Id { get; set; }
        public Guid Lock { get; set; }
        public byte Status { get; set; }

        static WorkflowProcessInstanceStatus()
        {
            DbTableName = "workflowprocessinstancestatus";
        }

        public WorkflowProcessInstanceStatus()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name="Lock", Type = MySqlDbType.Binary},
                new ColumnInfo {Name="Status", Type = MySqlDbType.Byte}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                   return Id.ToByteArray();
                case "Lock":
                   return Lock.ToByteArray();
                case "Status":
                    return Status.ToString();
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
                case "Lock":
                    Lock = new Guid((byte[])value);
                    break;
                case "Status":
                    Status = (byte)(sbyte)value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int MassChangeStatus(MySqlConnection connection, byte stateFrom, byte stateTo)
        {
            var command = string.Format("UPDATE {0} SET `Status` = @stateto WHERE `Status` = @statefrom", DbTableName);
            var p1 = new MySqlParameter("statefrom", MySqlDbType.Byte) {Value = stateFrom};
            var p2 = new MySqlParameter("stateto", MySqlDbType.Byte) {Value = stateTo};

            return ExecuteCommand(connection, command, p1, p2);
        }

        public static int ChangeStatus(MySqlConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            var command = string.Format("UPDATE {0} SET `Status` = @newstatus, `Lock` = @newlock WHERE `Id` = @id AND `Lock` = @oldlock", DbTableName);
            var p1 = new MySqlParameter("newstatus", MySqlDbType.Byte) { Value = status.Status };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = status.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.Binary) { Value = status.Id.ToByteArray() };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }
    }
}
