using System;
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

        static WorkflowProcessInstanceStatus()
        {
            DbTableName = "WorkflowProcessInstanceStatus";
        }

        public WorkflowProcessInstanceStatus()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="Lock", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="Status", Type = NpgsqlDbType.Smallint}
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
                    Status = (byte)(short)value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int MassChangeStatus(NpgsqlConnection connection, byte stateFrom, byte stateTo)
        {
            string command = string.Format("UPDATE {0} SET \"Status\" = @stateto WHERE \"Status\" = @statefrom", ObjectName);
            var p1 = new NpgsqlParameter("statefrom", NpgsqlDbType.Smallint) {Value = stateFrom};
            var p2 = new NpgsqlParameter("stateto", NpgsqlDbType.Smallint) {Value = stateTo};

            return ExecuteCommand(connection, command, p1, p2);
        }

        public static int ChangeStatus(NpgsqlConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            string command = string.Format("UPDATE {0} SET \"Status\" = @newstatus, \"Lock\" = @newlock WHERE \"Id\" = @id AND \"Lock\" = @oldlock", ObjectName);
            var p1 = new NpgsqlParameter("newstatus", NpgsqlDbType.Smallint) { Value = status.Status };
            var p2 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = status.Lock };
            var p3 = new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = status.Id };
            var p4 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }
    }
}
