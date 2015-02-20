using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NpgsqlTypes;

namespace OptimaJet.Workflow.PostgreSQL.Models
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
                new ColumnInfo(){Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo(){Name="ProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo(){Name="Name"},
                new ColumnInfo(){Name="NextExecutionDateTime", Type = NpgsqlDbType.Date },
                new ColumnInfo(){Name="Ignore", Type = NpgsqlDbType.Boolean },
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "ProcessId":
                    return ProcessId;
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
                    Id = (Guid)value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid)value;
                    break;
                case "Name":
                    Name = (string)value;
                    break;
                case "NextExecutionDateTime":
                    NextExecutionDateTime = (DateTime)value;
                    break;
                case "Ignore":
                    Ignore = (bool)value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int DeleteByProcessId(NpgsqlConnection connection, Guid processId, List<string> timersIgnoreList = null)
        {
            var p_processId = new NpgsqlParameter("processId", NpgsqlDbType.Uuid);
            p_processId.Value = processId;

            var p_timerIgnoreList = new NpgsqlParameter("timerIgnoreList", NpgsqlDbType.Array | NpgsqlDbType.Text);
            p_timerIgnoreList.Value = timersIgnoreList != null ? timersIgnoreList.ToArray() : new string[]{};

            return ExecuteCommand(connection,
                string.Format("DELETE FROM \"{0}\" WHERE \"ProcessId\" = @processid AND \"Name\" != ANY(@timerIgnoreList)", _tableName), p_processId, p_timerIgnoreList);
        }

        public static WorkflowProcessTimer SelectByProcessIdAndName(NpgsqlConnection connection, Guid processId, string name)
        {
            string selectText = string.Format("SELECT * FROM \"{0}\" WHERE \"ProcessId\" = @processid AND \"Name\" = @name", _tableName);
            var p1 = new NpgsqlParameter("processId", NpgsqlDbType.Uuid);
            p1.Value = processId;

            var p2 = new NpgsqlParameter("name", NpgsqlDbType.Varchar);
            p2.Value = name;
            return Select(connection, selectText, p1, p2).FirstOrDefault();
        }

        public static int ClearTimersIgnore(NpgsqlConnection connection)
        {
            string command = string.Format("UPDATE \"{0}\" SET \"Ignore\" = FALSE WHERE \"Ignore\" = TRUE", _tableName);
            return ExecuteCommand(connection, command);
        }

        public static WorkflowProcessTimer GetCloseExecutionTimer(NpgsqlConnection connection)
        {
            string selectText = string.Format("SELECT * FROM \"{0}\" WHERE \"Ignore\" = FALSE ORDER BY \"NextExecutionDateTime\" LIMIT 1", _tableName);
            return Select(connection, selectText).FirstOrDefault();
        }

        public static WorkflowProcessTimer[] GetTimersToExecute(NpgsqlConnection connection, DateTime now)
        {
            string selectText = string.Format("SELECT * FROM \"{0}\" WHERE \"Ignore\" = FALSE AND \"NextExecutionDateTime\" <= @currentTime", _tableName);
            var p = new NpgsqlParameter("currentTime", NpgsqlDbType.Date);
            p.Value = now;
            return Select(connection, selectText, p);
        }

        public static int SetIgnore(NpgsqlConnection connection, WorkflowProcessTimer[] timers)
        {
            if (timers.Length == 0)
                return 0;

            var p = new NpgsqlParameter("timerListParam", NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            p.Value = timers.Select(c => c.Id).ToArray();
            return ExecuteCommand(connection, string.Format("DELETE FROM \"{0}\" WHERE \"Id\" = ANY(@timerListParam)", _tableName), p);
        }
    }
}
