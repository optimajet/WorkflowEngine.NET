using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessTimer : DbObject<WorkflowProcessTimer>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string Name { get; set; }
        public DateTime NextExecutionDateTime { get; set; }
        public bool Ignore { get; set; }

        private const string _tableName = "WorkflowProcessTimer";

        public WorkflowProcessTimer()
            : base()
        {
            db_TableName = _tableName;
            db_Columns.AddRange(new ColumnInfo[]{
                new ColumnInfo(){Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo(){Name="ProcessId", Type = OracleDbType.Raw},
                new ColumnInfo(){Name="Name"},
                new ColumnInfo(){Name="NextExecutionDateTime", Type = OracleDbType.Date },
                new ColumnInfo(){Name="Ignore", Type = OracleDbType.Byte },
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "ProcessId":
                    return ProcessId.ToByteArray();
                case "Name":
                    return Name;
                case "NextExecutionDateTime":
                    return NextExecutionDateTime;
                case "Ignore":
                    return Ignore ? (string)"1" : (string)"0";
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
                case "ProcessId":
                    ProcessId = new Guid((byte[])value);
                    break;
                case "Name":
                    Name = (string)value;
                    break;
                case "NextExecutionDateTime":
                    NextExecutionDateTime = (DateTime)value;
                    break;
                case "Ignore":
                    Ignore = (string)value == "1";
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int DeleteByProcessId(OracleConnection connection, Guid processId, List<string> timersIgnoreList = null)
        {
            string timerIgnoreListParam = timersIgnoreList != null ? string.Join(",", timersIgnoreList.Select(c=> string.Format("'{0}'", c))): "";

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE PROCESSID = :processid AND NAME not in (:timerIgnoreList)", _tableName),
                new OracleParameter("processId", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input),
                    new OracleParameter("timerIgnoreList", OracleDbType.NVarchar2, timerIgnoreListParam, ParameterDirection.Input)
                    );
        }

        public static WorkflowProcessTimer SelectByProcessIdAndName(OracleConnection connection, Guid processId, string name)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE PROCESSID = :processid AND NAME = :name", _tableName);
    
            return Select(connection, selectText,
                new OracleParameter("processId", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input),
                new OracleParameter("name", OracleDbType.NVarchar2, name, ParameterDirection.Input))
                .FirstOrDefault();
        }

        public static int ClearTimersIgnore(OracleConnection connection)
        {
            string command = string.Format("UPDATE {0} SET IGNORE = 0 WHERE IGNORE = 1", _tableName);
            return ExecuteCommand(connection, command);
        }

        public static WorkflowProcessTimer GetCloseExecutionTimer(OracleConnection connection)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE IGNORE = 0 AND ROWNUM = 1 ORDER BY NextExecutionDateTime", _tableName);
            return Select(connection, selectText).FirstOrDefault();
        }

        public static WorkflowProcessTimer[] GetTimersToExecute(OracleConnection connection, DateTime now)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE IGNORE = 0 AND NextExecutionDateTime <= :currentTime", _tableName);
            return Select(connection, selectText,
                new OracleParameter("currentTime", OracleDbType.Date, now, ParameterDirection.Input));
        }

        public static int SetIgnore(OracleConnection connection, WorkflowProcessTimer[] timers)
        {
            if (timers.Length == 0)
                return 0;

            string timerListParam = string.Join(",", timers.Select(c => string.Format("'{0}'", c.Id)));

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE ID in (:timerListParam)", _tableName),
                new OracleParameter("timerListParam", OracleDbType.NVarchar2, timerListParam, ParameterDirection.Input));
        }
    }
}
