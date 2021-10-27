using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
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
                new ColumnInfo {Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name="ProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name="Name"},
                new ColumnInfo {Name="NextExecutionDateTime", Type = OracleDbType.TimeStamp },
                new ColumnInfo {Name="Ignore", Type = OracleDbType.Byte },
                new ColumnInfo {Name=nameof(RootProcessId), Type = OracleDbType.Raw},
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
                    return Ignore ? "1" : "0";
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
                    Ignore = (string)value == "1";
                    break;
                case nameof(RootProcessId):
                    RootProcessId = new Guid((byte[])value);
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public static async Task<int> DeleteInactiveByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            var pProcessId = new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input);

            return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE PROCESSID = :processid AND IGNORE = 1", pProcessId).ConfigureAwait(false);
        }

        public static async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId, List<string> timersIgnoreList = null)
        {
            var pProcessId = new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input);

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<OracleParameter>() {pProcessId};
                int cnt = 0;
                foreach (string timer in timersIgnoreList)
                {
                    string parameterName = $"ignore{cnt}";
                    parameters.Add($":{parameterName}");
                    sqlParameters.Add(new OracleParameter(parameterName, OracleDbType.NVarchar2, timer, ParameterDirection.Input));
                    cnt++;
                }

                string commandText = $"DELETE FROM {ObjectName} WHERE PROCESSID = :processid AND NAME NOT IN ({String.Join(",", parameters)})";

                return await ExecuteCommandNonQueryAsync(connection, commandText, sqlParameters.ToArray()).ConfigureAwait(false);
            }

            return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE PROCESSID = :processid", pProcessId).ConfigureAwait(false);

        }

        public static async Task<WorkflowProcessTimer> SelectByProcessIdAndNameAsync(OracleConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE PROCESSID = :processid AND NAME = :name";
    
            return (await SelectAsync(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input),
                new OracleParameter("name", OracleDbType.NVarchar2, name, ParameterDirection.Input)).ConfigureAwait(false))
                .FirstOrDefault();
        }

        public static async Task<IEnumerable<WorkflowProcessTimer>> SelectByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE PROCESSID = :processid";

            return await SelectAsync(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input)).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<WorkflowProcessTimer>> SelectActiveByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE PROCESSID = :processid AND IGNORE = 0";

            return await SelectAsync(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input)).ConfigureAwait(false);
        }

        public static async Task<int> SetTimerIgnoreAsync(OracleConnection connection, Guid timerId)
        {
            string command = $"UPDATE {ObjectName} SET IGNORE = 1 WHERE ID = :timerid AND IGNORE = 0";
            var p1 = new OracleParameter("timerid", OracleDbType.Raw, timerId.ToByteArray(), ParameterDirection.Input);
            return await ExecuteCommandNonQueryAsync(connection, command, p1).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessTimer[]> GetTopTimersToExecuteAsync(OracleConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM (SELECT * FROM {ObjectName} " +
                                "WHERE IGNORE = 0 AND NextExecutionDateTime <= :currentTime " +
                                "ORDER BY NextExecutionDateTime) WHERE ROWNUM <= :rowsCount";

            return await SelectAsync(connection, selectText,
                new OracleParameter("currentTime", OracleDbType.Date, now, ParameterDirection.Input),
                new OracleParameter("rowsCount", OracleDbType.Int32, top, ParameterDirection.Input)
            ).ConfigureAwait(false);
        }
    }
}
