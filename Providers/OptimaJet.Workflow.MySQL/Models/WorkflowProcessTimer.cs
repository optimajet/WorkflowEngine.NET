using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
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
            DbTableName = "workflowprocesstimer";
        }

        public WorkflowProcessTimer()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name="ProcessId", Type = MySqlDbType.Binary},
                new ColumnInfo {Name="Name"},
                new ColumnInfo {Name="NextExecutionDateTime", Type = MySqlDbType.DateTime },
                new ColumnInfo {Name="Ignore", Type = MySqlDbType.Bit },
                new ColumnInfo {Name=nameof(RootProcessId), Type = MySqlDbType.Binary},
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
                case nameof(RootProcessId):
                    return RootProcessId.ToByteArray();
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
                case nameof(RootProcessId):
                    RootProcessId = new Guid((byte[])value);
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<int> DeleteInactiveByProcessIdAsync(MySqlConnection connection, Guid processId, MySqlTransaction transaction = null)
        {
            var pProcessId = new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()};

            return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {DbTableName} WHERE `ProcessId` = @processid AND `Ignore` = 1", transaction, pProcessId).ConfigureAwait(false);
        }

        public static async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId,
            List<string> timersIgnoreList = null, MySqlTransaction transaction = null)
        {
            var pProcessId = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<MySqlParameter>() { pProcessId };
                int cnt = 0;
                foreach (string timer in timersIgnoreList)
                {
                    string parameterName = $"ignore{cnt}";
                    parameters.Add($"@{parameterName}");
                    sqlParameters.Add(new MySqlParameter(parameterName, MySqlDbType.VarString) {Value = timer});
                    cnt++;
                }

                string commandText = $"DELETE FROM {DbTableName} WHERE `ProcessId` = @processid AND `Name` not in ({string.Join(",", parameters)})";

                return await ExecuteCommandNonQueryAsync(connection, commandText, sqlParameters.ToArray()).ConfigureAwait(false);
            }

            return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {DbTableName} WHERE `ProcessId` = @processid", pProcessId).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessTimer> SelectByProcessIdAndNameAsync(MySqlConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `ProcessId` = @processid AND `Name` = @name";
            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            var p2 = new MySqlParameter("name", MySqlDbType.VarString) {Value = name};
            return (await SelectAsync(connection, selectText, p1, p2).ConfigureAwait(false)).FirstOrDefault();
        }

        public static async Task<IEnumerable<WorkflowProcessTimer>> SelectByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `ProcessId` = @processid";

            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<WorkflowProcessTimer>> SelectActiveByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `ProcessId` = @processid AND `Ignore` = 0";

            var p1 = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public static async Task<int> SetTimerIgnoreAsync(MySqlConnection connection, Guid timerId)
        {
            string command = $"UPDATE {DbTableName} SET `Ignore` = 1 WHERE `Id` = @timerid AND `Ignore` = 0";
            var p1 = new MySqlParameter("timerid", MySqlDbType.Binary) { Value = timerId.ToByteArray() };
            return await ExecuteCommandNonQueryAsync(connection, command, p1).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessTimer[]> GetTopTimersToExecuteAsync(MySqlConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM {DbTableName} " +
                                "WHERE `Ignore` = 0 AND `NextExecutionDateTime` <= @currentTime " +
                                $"ORDER BY `NextExecutionDateTime` LIMIT {top}";

            var p1 = new MySqlParameter("currentTime", MySqlDbType.DateTime) { Value = now };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }
    }
}
