using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace OptimaJet.Workflow.MySQL
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
                new ColumnInfo(){Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo(){Name="ProcessId", Type = MySqlDbType.Binary},
                new ColumnInfo(){Name="Name"},
                new ColumnInfo(){Name="NextExecutionDateTime", Type = MySqlDbType.DateTime },
                new ColumnInfo(){Name="Ignore", Type = MySqlDbType.Bit },
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
                    return Ignore;
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
                    Ignore = value.ToString() == "1";
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int DeleteByProcessId(MySqlConnection connection, Guid processId,
            List<string> timersIgnoreList = null, MySqlTransaction transaction = null)
        {
            var timerIgnoreListParam = timersIgnoreList != null
                ? string.Join(",", timersIgnoreList.Select(c => string.Format("`{0}`", c)))
                : "";

            var pProcessId = new MySqlParameter("processId", MySqlDbType.Binary) {Value = processId.ToByteArray()};

            var pTimerIgnoreList = new MySqlParameter("timerIgnoreList", MySqlDbType.VarString)
            {
                Value = timerIgnoreListParam
            };

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE `ProcessId` = @processid AND `Name` not in (@timerIgnoreList)",
                    _tableName), transaction, pProcessId, pTimerIgnoreList);
        }

        public static WorkflowProcessTimer SelectByProcessIdAndName(MySqlConnection connection, Guid processId, string name)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE `ProcessId` = @processid AND `Name` = @name", _tableName);
            var p1 = new MySqlParameter("processId", MySqlDbType.Binary);
            p1.Value = processId.ToByteArray();

            var p2 = new MySqlParameter("name", MySqlDbType.VarString);
            p2.Value = name;
            return Select(connection, selectText, p1, p2).FirstOrDefault();
        }

        public static int ClearTimersIgnore(MySqlConnection connection)
        {
            string command = string.Format("UPDATE {0} SET `Ignore` = 0 WHERE `Ignore` = 1", _tableName);
            return ExecuteCommand(connection, command);
        }

        public static WorkflowProcessTimer GetCloseExecutionTimer(MySqlConnection connection)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE `Ignore` = 0 ORDER BY `NextExecutionDateTime` LIMIT 1", _tableName);
            var parameters = new MySqlParameter[]{};

            return Select(connection, selectText, parameters).FirstOrDefault();
        }

        public static WorkflowProcessTimer[] GetTimersToExecute(MySqlConnection connection, DateTime now)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE `Ignore` = 0 AND `NextExecutionDateTime` <= @currentTime", _tableName);
            var p = new MySqlParameter("currentTime", MySqlDbType.DateTime);
            p.Value = now;
            return Select(connection, selectText, p);
        }

        public static int SetIgnore(MySqlConnection connection, WorkflowProcessTimer[] timers)
        {
            if (timers.Length == 0)
                return 0;

            string timerListParam = string.Join(",", timers.Select(c => string.Format("`{0}`", c.Id)));
            var p = new MySqlParameter("timerListParam",  MySqlDbType.VarString);
            p.Value = timerListParam;
            return ExecuteCommand(connection, string.Format("DELETE FROM {0} WHERE `Id` in (@timerListParam)", _tableName), p);
        }
    }
}
