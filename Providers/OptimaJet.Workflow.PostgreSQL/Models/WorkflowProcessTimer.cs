using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = NpgsqlDbType.Uuid}, new ColumnInfo {Name = "ProcessId", Type = NpgsqlDbType.Uuid}, new ColumnInfo {Name = "Name"},
                new ColumnInfo {Name = "NextExecutionDateTime", Type = NpgsqlDbType.Timestamp}, new ColumnInfo {Name = "Ignore", Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = nameof(RootProcessId), Type = NpgsqlDbType.Uuid},
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

        public static async Task<int> DeleteInactiveByProcessIdAsync(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction = null)
        {
            var pProcessId = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};

            return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE \"ProcessId\" = @processid AND \"Ignore\" = TRUE", transaction, pProcessId).ConfigureAwait(false);
        }

        public static async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId, List<string> timersIgnoreList = null, NpgsqlTransaction transaction = null)
        {
            var pProcessId = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                var pTimerIgnoreList = new NpgsqlParameter("timerIgnoreList", NpgsqlDbType.Array | NpgsqlDbType.Varchar) //-V3059
                {
                    Value = timersIgnoreList.ToArray()
                };

                return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} WHERE \"ProcessId\" = @processid AND \"Name\" != ALL(@timerIgnoreList)", transaction, pProcessId, pTimerIgnoreList).ConfigureAwait(false);
            }

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {ObjectName} WHERE \"ProcessId\" = @processid", transaction, pProcessId).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessTimer> SelectByProcessIdAndNameAsync(NpgsqlConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"ProcessId\" = @processid AND \"Name\" = @name";
            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};
            var p2 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) {Value = name};
            return (await SelectAsync(connection, selectText, p1, p2).ConfigureAwait(false)).FirstOrDefault();
        }

        public static async Task<IEnumerable<WorkflowProcessTimer>> SelectByProcessIdAsync(NpgsqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"ProcessId\" = @processid";

            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<WorkflowProcessTimer>> SelectActiveByProcessIdAsync(NpgsqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"ProcessId\" = @processid AND \"Ignore\" = FALSE";

            var p1 = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public static async Task<int> SetTimerIgnoreAsync(NpgsqlConnection connection, Guid timerId)
        {
            string command = $"UPDATE {ObjectName} SET \"Ignore\" = TRUE WHERE \"Id\" = @timerid AND \"Ignore\" = FALSE";
            var p1 = new NpgsqlParameter("timerid", NpgsqlDbType.Uuid) {Value = timerId};
            return await ExecuteCommandNonQueryAsync(connection, command, p1).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessTimer[]> GetTopTimersToExecuteAsync(NpgsqlConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM {ObjectName} " +
                                "WHERE \"Ignore\" = FALSE AND \"NextExecutionDateTime\" <= @currentTime " +
                                $"ORDER BY \"NextExecutionDateTime\" LIMIT {top}";

            var p1 = new NpgsqlParameter("currentTime", NpgsqlDbType.Timestamp) {Value = now};

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
