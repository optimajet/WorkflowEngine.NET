using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle.Models
{
    public class WorkflowSync : DbObject<WorkflowSync>
    {
        static WorkflowSync()
        {
            DbTableName = "WorkflowSync";
        }

        public WorkflowSync()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Name", IsKey = true, Type = OracleDbType.NVarchar2, Size = 450},
                new ColumnInfo {Name = "LOCKFLAG", Type = OracleDbType.Raw}
            });
        }

        public string Name { get; set; }
        public Guid LOCKFLAG { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Name":
                    return Name;
                case "LOCKFLAG":
                    return LOCKFLAG.ToByteArray();
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "LOCKFLAG":
                    LOCKFLAG = new Guid((byte[])value);
                    break;
                case "Name":
                    Name = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static WorkflowSync GetByName(OracleConnection connection, string name)
        {
            var selectText = String.Format("SELECT * FROM {0} WHERE NAME = :name", ObjectName);
            var locks = Select(connection, selectText, new OracleParameter("name", OracleDbType.NVarchar2, name, ParameterDirection.Input));

            return locks.FirstOrDefault();
        }

        public static int UpdateLock(OracleConnection connection, string name, Guid oldLock, Guid newLock)
        {
            var command = String.Format("UPDATE {0} SET LOCKFLAG = :newlock WHERE NAME = :name AND LOCKFLAG = :oldlock", ObjectName);
            var p1 = new OracleParameter("newlock", OracleDbType.Raw, newLock.ToByteArray(), ParameterDirection.Input);
            var p2 = new OracleParameter("oldlock", OracleDbType.Raw, oldLock.ToByteArray(), ParameterDirection.Input);
            var p3 = new OracleParameter("name", OracleDbType.NVarchar2, name, ParameterDirection.Input);

            return ExecuteCommand(connection, command, p1, p2, p3);
        }
    }
}
