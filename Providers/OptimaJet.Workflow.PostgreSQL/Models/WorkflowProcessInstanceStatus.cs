using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
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
            DbTableName = "WorkflowProcessInstanceStatus";
        }

        public WorkflowProcessInstanceStatus()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="Lock", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="Status", Type = NpgsqlDbType.Smallint},
                new ColumnInfo {Name = "RuntimeId"},
                new ColumnInfo {Name = "SetTime", Type = NpgsqlDbType.Timestamp}
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
                    Id = (Guid)value;
                    break;
                case "Lock":
                    Lock = (Guid)value;
                    break;
                case "Status":
                    Status = (byte)(short)value;
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

        public static List<Guid> GetProcessesByStatus(NpgsqlConnection connection, byte status, string runtimeId = null)
        {
            string command = String.Format("SELECT \"Id\" FROM {0} WHERE \"Status\" = @status", ObjectName);

            var p = new List<NpgsqlParameter>();

            if (!String.IsNullOrEmpty(runtimeId))
            {
                command += " AND \"RuntimeId\" = @runtime";
                p.Add(new NpgsqlParameter("runtime", NpgsqlDbType.Varchar) { Value = runtimeId });
            }

            p.Add(new NpgsqlParameter("status", NpgsqlDbType.Smallint) { Value = status });
            return Select(connection, command, p.ToArray()).Select(s => s.Id).ToList();
        }

        public static int MassChangeStatus(NpgsqlConnection connection, byte stateFrom, byte stateTo, DateTime time)
        {
            string command = string.Format("UPDATE {0} SET \"Status\" = @stateto, \"SetTime\" = @settime WHERE \"Status\" = @statefrom", ObjectName);
            var p1 = new NpgsqlParameter("statefrom", NpgsqlDbType.Smallint) {Value = stateFrom};
            var p2 = new NpgsqlParameter("stateto", NpgsqlDbType.Smallint) {Value = stateTo};
            var p3 = new NpgsqlParameter("settime", NpgsqlDbType.Timestamp) { Value = time };

            return ExecuteCommand(connection, command, p1, p2, p3);
        }

        public static int ChangeStatus(NpgsqlConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            string command = string.Format(
                "UPDATE {0} SET \"Status\" = @newstatus, \"Lock\" = @newlock, \"SetTime\" = @settime, \"RuntimeId\" = @runtimeid " + 
                "WHERE \"Id\" = @id AND \"Lock\" = @oldlock", ObjectName);
            var p1 = new NpgsqlParameter("newstatus", NpgsqlDbType.Smallint) { Value = status.Status };
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = status.Lock };
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = status.Id };
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };
            var p5 = new NpgsqlParameter("settime", NpgsqlDbType.Timestamp) { Value = status.SetTime };
            var p6 = new NpgsqlParameter("runtimeid", NpgsqlDbType.Varchar) { Value = status.RuntimeId };

            return ExecuteCommand(connection, command, p1, p2, p3, p4, p5, p6);
        }
    }
}
