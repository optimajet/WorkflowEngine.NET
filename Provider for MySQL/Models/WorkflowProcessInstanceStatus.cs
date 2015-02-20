using System;
using MySql.Data.MySqlClient;

namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        public Guid Id { get; set; }
        public Guid Lock { get; set; }
        public byte Status { get; set; }

        private static string _tableName = "WorkflowProcessInstanceStatus";

        public WorkflowProcessInstanceStatus()
            : base()
        {
            db_TableName = _tableName;
            db_Columns.AddRange(new ColumnInfo[]{
                new ColumnInfo(){Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo(){Name="Lock", Type = MySqlDbType.Binary},
                new ColumnInfo(){Name="Status", Type = MySqlDbType.Byte}
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
            string command = string.Format("UPDATE {0} SET `Status` = @stateto WHERE `Status` = @statefrom", _tableName);
            var p1 = new MySqlParameter("statefrom", MySqlDbType.Byte);
            p1.Value = stateFrom;
            var p2 = new MySqlParameter("stateto", MySqlDbType.Byte);
            p2.Value = stateTo;

            return ExecuteCommand(connection, command, p1, p2);
        }
    }
}
