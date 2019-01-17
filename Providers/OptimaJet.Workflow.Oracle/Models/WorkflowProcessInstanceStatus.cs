using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        public Guid Id { get; set; }
        public Guid LOCKFLAG { get; set; }
        public short Status { get; set; }

        static WorkflowProcessInstanceStatus()
        {
            DbTableName = "WorkflowProcessInstanceS";
        }

        public WorkflowProcessInstanceStatus()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = "LOCKFLAG", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "Status", Type = OracleDbType.Int16}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                   return Id.ToByteArray();
                case "LOCKFLAG":
                    return LOCKFLAG.ToByteArray();
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
                    Id = new Guid((byte[])value);
                    break;
                case "LOCKFLAG":
                    LOCKFLAG = new Guid((byte[])value);
                    break;
                case "Status":
                    Status = (short) value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int MassChangeStatus(OracleConnection connection, byte stateFrom, byte stateTo)
        {
            string command = string.Format("UPDATE {0} SET Status = :stateto WHERE Status = :statefrom", ObjectName);
            return ExecuteCommand(connection, command,
                new OracleParameter("stateto", OracleDbType.Int16, stateTo, ParameterDirection.Input),
                new OracleParameter("statefrom", OracleDbType.Int16, stateFrom, ParameterDirection.Input)
            );
        }

        public static int ChangeStatus(OracleConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            var command = string.Format("UPDATE {0} SET Status = :newstatus, LOCKFLAG = :newlock WHERE Id = :id AND LOCKFLAG = :oldlock", ObjectName);
            var p1 = new OracleParameter("newstatus", OracleDbType.Int16, status.Status, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, status.LOCKFLAG.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.Raw, status.Id.ToByteArray(), ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }
    }
}
