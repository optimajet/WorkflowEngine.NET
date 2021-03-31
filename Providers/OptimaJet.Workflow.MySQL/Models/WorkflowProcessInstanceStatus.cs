using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task<List<Guid>> GetProcessesByStatusAsync(MySqlConnection connection, byte status, string runtimeId = null)
        {
            string command = $"SELECT `Id` FROM {DbTableName} WHERE `Status` = @status";
            var p = new List<MySqlParameter>();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += " AND `RuntimeId` = @runtime";
                p.Add(new MySqlParameter("runtime", MySqlDbType.VarChar) { Value = runtimeId });
            }

            p.Add(new MySqlParameter("status", MySqlDbType.Byte) { Value = status });
            return (await SelectAsync(connection, command, p.ToArray()).ConfigureAwait(false)).Select(s => s.Id).ToList();
        }

        public static async Task<int> ChangeStatusAsync(MySqlConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            string command = $"UPDATE {DbTableName} SET `Status` = @newstatus, `Lock` = @newlock, `SetTime` = @settime, `RuntimeId` = @runtimeid WHERE `Id` = @id AND `Lock` = @oldlock";
            var p1 = new MySqlParameter("newstatus", MySqlDbType.Byte) { Value = status.Status };
            var p2 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = status.Lock.ToByteArray() };
            var p3 = new MySqlParameter("id", MySqlDbType.Binary) { Value = status.Id.ToByteArray() };
            var p4 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };
            var p5 = new MySqlParameter("settime", MySqlDbType.DateTime) { Value = status.SetTime };
            var p6 = new MySqlParameter("runtimeid", MySqlDbType.VarChar) { Value = status.RuntimeId };

            return await ExecuteCommandNonQueryAsync(connection, command, p1, p2, p3, p4, p5, p6).ConfigureAwait(false);
        }
    }
}
