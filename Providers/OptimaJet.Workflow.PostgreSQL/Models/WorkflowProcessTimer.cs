using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessTimer : DbObject<WorkflowProcessTimer>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public string Name { get; set; }
        public DateTime NextExecutionDateTime { get; set; }
        public bool Ignore { get; set; }

        static WorkflowProcessTimer()
        {
            DbTableName = "WorkflowProcessTimer";
        }

        public WorkflowProcessTimer()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="ProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="Name"},
                new ColumnInfo {Name="NextExecutionDateTime", Type = NpgsqlDbType.Timestamp },
                new ColumnInfo {Name="Ignore", Type = NpgsqlDbType.Boolean },
                new ColumnInfo {Name=nameof(RootProcessId), Type = NpgsqlDbType.Uuid},
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
                case nameof(RootProcessId):
                    return RootProcessId;
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
                case nameof(RootProcessId):
                    RootProcessId = (Guid)value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int DeleteInactiveByProcessId(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction = null)
        {
            var pProcessId = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE \"ProcessId\" = @processid AND \"Ignore\" = TRUE", ObjectName), transaction, pProcessId);
        }

        public static int DeleteByProcessId(NpgsqlConnection connection, Guid processId, List<string> timersIgnoreList = null, NpgsqlTransaction transaction = null)
        {
            var pProcessId = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                var pTimerIgnoreList = new NpgsqlParameter("timerIgnoreList", NpgsqlDbType.Array | NpgsqlDbType.Varchar) //-V3059
                {
                    Value =timersIgnoreList.ToArray()
                };

                return ExecuteCommand(connection,
                    string.Format("DELETE FROM {0} WHERE \"ProcessId\" = @processid AND \"Name\" != ALL(@timerIgnoreList)", ObjectName), transaction, pProcessId, pTimerIgnoreList);
            }
            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE \"ProcessId\" = @processid", ObjectName), transaction, pProcessId);
        }

        public static WorkflowProcessTimer SelectByProcessIdAndName(NpgsqlConnection connection, Guid processId, string name)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE \"ProcessId\" = @processid AND \"Name\" = @name", ObjectName);
            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            var p2 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) {Value = name};
            return Select(connection, selectText, p1, p2).FirstOrDefault();
        }

        public static IEnumerable<WorkflowProcessTimer> SelectByProcessId(NpgsqlConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE \"ProcessId\" = @processid", ObjectName);

            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            return Select(connection, selectText, p1);
        }

        public static IEnumerable<WorkflowProcessTimer> SelectActiveByProcessId(NpgsqlConnection connection, Guid processId)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE \"ProcessId\" = @processid AND \"Ignore\" = FALSE", ObjectName);

            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            return Select(connection, selectText, p1);
        }

        public static int ClearTimerIgnore(NpgsqlConnection connection, Guid timerId)
        {
            var command = string.Format("UPDATE {0} SET \"Ignore\" = FALSE WHERE \"Id\" = @timerid", ObjectName);
            var p1 = new NpgsqlParameter("timerid", NpgsqlDbType.Uuid) { Value = timerId };
            return ExecuteCommand(connection, command, p1);
        }

        public static int SetTimerIgnore(NpgsqlConnection connection, Guid timerId)
        {
            var command = string.Format("UPDATE {0} SET \"Ignore\" = TRUE WHERE \"Id\" = @timerid AND \"Ignore\" = FALSE", ObjectName);
            var p1 = new NpgsqlParameter("timerid", NpgsqlDbType.Uuid) { Value = timerId };
            return ExecuteCommand(connection, command, p1);
        }

        public static WorkflowProcessTimer GetCloseExecutionTimer(NpgsqlConnection connection)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE \"Ignore\" = FALSE ORDER BY \"NextExecutionDateTime\" LIMIT 1", ObjectName);
            return Select(connection, selectText).FirstOrDefault();
        }

        public static WorkflowProcessTimer[] GetTimersToExecute(NpgsqlConnection connection, DateTime now)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE \"Ignore\" = FALSE AND \"NextExecutionDateTime\" <= @currentTime", ObjectName);
            var p = new NpgsqlParameter("currentTime", NpgsqlDbType.Timestamp) { Value = now };
            return Select(connection, selectText, p);
        }

        public static WorkflowProcessTimer[] GetTopTimersToExecute(NpgsqlConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                "WHERE \"Ignore\" = FALSE AND \"NextExecutionDateTime\" <= @currentTime " +
                                $"ORDER BY \"NextExecutionDateTime\" LIMIT {top}";

            var p1 = new NpgsqlParameter("currentTime", NpgsqlDbType.Timestamp) { Value = now };
           
            return Select(connection, selectText, p1);
        }

        public static int SetIgnore(NpgsqlConnection connection, WorkflowProcessTimer[] timers)
        {
            if (timers.Length == 0)
                return 0;

            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            var p = new NpgsqlParameter("timerListParam", NpgsqlDbType.Array | NpgsqlDbType.Uuid) {Value = timers.Select(c => c.Id).ToArray()}; //-V3059
            return ExecuteCommand(connection, string.Format("UPDATE {0} SET \"Ignore\" = TRUE WHERE \"Id\" = ANY(@timerListParam)", ObjectName), p);
        }
    }
}
