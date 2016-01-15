using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        public Guid Id { get; set; }
        public Guid LOCKFLAG { get; set; }
        public byte Status { get; set; }

        private static string _tableName = "WorkflowProcessInstanceS";

        public WorkflowProcessInstanceStatus()
            : base()
        {
            db_TableName = _tableName;
            db_Columns.AddRange(new ColumnInfo[]{
                new ColumnInfo(){Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo(){Name="LOCKFLAG", Type = OracleDbType.Raw},
                new ColumnInfo(){Name="Status", Type = OracleDbType.Char}
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
                case "LOCKFLAG":
                    LOCKFLAG = new Guid((byte[])value);
                    break;
                case "Status":
                    Status = byte.Parse((string)value);
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int MassChangeStatus(OracleConnection connection, byte stateFrom, byte stateTo)
        {
            string command = string.Format("UPDATE {0} SET STATUS = :stateto WHERE STATUS = :statefrom", _tableName);
            return ExecuteCommand(connection, command,
                new OracleParameter("statefrom", OracleDbType.Byte, stateFrom, ParameterDirection.Input),
                    new OracleParameter("stateto", OracleDbType.Byte, stateTo, ParameterDirection.Input));
        }

        public static int ChangeStatus(OracleConnection connection, WorkflowProcessInstanceStatus status, Guid oldLock)
        {
            var command = string.Format("UPDATE {0} SET Status = :newstatus, Lock = :newlock WHERE Id = :id AND Lock = :oldlock", _tableName);
            var p1 = new OracleParameter("newstatus", OracleDbType.Byte, status.Status, ParameterDirection.Input);
            var p2 = new OracleParameter("newlock", OracleDbType.Raw, status.LOCKFLAG.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("id", OracleDbType.Raw, status.Id.ToByteArray(), ParameterDirection.Input);
            var p4 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);

            return ExecuteCommand(connection, command, p1, p2, p3, p4);
        }
    }
}
