using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        public Guid Id { get; set; }
        public Guid Lock { get; set; }
        public byte Status { get; set; }

        public string RuntimeId { get; set; }

        public DateTime SetTime { get; set; }

        static WorkflowProcessInstanceStatus()
        {
            DbTableName = "workflowprocessinstancestatus";
        }

        public WorkflowProcessInstanceStatus()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name="Lock", Type = MySqlDbType.Binary},
                new ColumnInfo {Name="Status", Type = MySqlDbType.Byte},
                new ColumnInfo {Name = "RuntimeId", Type = MySqlDbType.VarChar},
                new ColumnInfo {Name = "SetTime", Type = MySqlDbType.DateTime}
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
                    Id = new Guid((byte[])value);
                    break;
                case "Lock":
                    Lock = new Guid((byte[])value);
                    break;
                case "Status":
                    Status = (byte)(sbyte)value;
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

        public static List<Guid> GetProcessesByStatus(MySqlConnection connection, byte status, string runtimeId = null)
        {
            string command = String.Format("SELECT `Id` FROM {0} WHERE `Status` = @status", DbTableName);
            var p = new List<MySqlParameter>();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += " AND `RuntimeId` = @runtime";
                p.Add(new MySqlParameter("runtime", MySqlDbType.VarChar) { Value = runtimeId });
            }

            p.Add(new MySqlParameter("status", MySqlDbType.Byte) { Value = status });
            return Select(connection, command, p.ToArray()).Select(s => s.Id).ToList();
        }

        public static int MassChangeStatus(MySqlConnection connection, byte stateFrom, byte stateTo, DateTime time)
        {
            var command = string.Format("UPDATE {0} SET `Status` = @stateto, `SetTime` = @settime WHERE `Status` = @statefrom", DbTableName);
            var p1 = new MySqlParameter("statefrom", MySqlDbType.Byte) {Value = stateFrom};
            var p2 = new MySqlParameter("stateto", MySqlDbType.Byte) {Value = stateTo};
            var p3 = new MySqlParameter("settime", MySqlDbType.DateTime) { Value = time };

            return ExecuteCommand(connection, command, p1, p2, p3);
        }

        public static int ChangeStatus(MySqlConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            var command = string.Format("UPDATE {0} SET `Status` = @newstatus, `Lock` = @newlock, `SetTime` = @settime, `RuntimeId` = @runtimeid WHERE `Id` = @id AND `Lock` = @oldlock", DbTableName);
            var p1 = new MySqlParameter("newstatus", MySqlDbType.Byte) { Value = status.Status };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = status.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.Binary) { Value = status.Id.ToByteArray() };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };
            var p5 = new MySqlParameter("settime", MySqlDbType.DateTime) { Value = status.SetTime };
            var p6 = new MySqlParameter("runtimeid", MySqlDbType.VarChar) { Value = status.RuntimeId };

            return ExecuteCommand(connection, command, p1, p2, p3, p4, p5, p6);
        }
    }
}
