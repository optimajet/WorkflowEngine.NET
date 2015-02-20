using System;
using Npgsql;
using NpgsqlTypes;

namespace OptimaJet.Workflow.PostgreSQL
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
                new ColumnInfo(){Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo(){Name="Lock", Type = NpgsqlDbType.Uuid},
                new ColumnInfo(){Name="Status", Type = NpgsqlDbType.Smallint}
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
            string command = string.Format("UPDATE \"{0}\" SET \"Status\" = @stateto WHERE \"Status\" = @statefrom", _tableName);
            var p1 = new NpgsqlParameter("statefrom", NpgsqlDbType.Smallint);
            p1.Value = stateFrom;
            var p2 = new NpgsqlParameter("stateto", NpgsqlDbType.Smallint);
            p2.Value = stateTo;

            return ExecuteCommand(connection, command, p1, p2);
        }
    }
}
