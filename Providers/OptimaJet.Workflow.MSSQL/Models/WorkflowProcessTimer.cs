using System;
using System.Collections.Generic;
using System.Data;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Fault;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessTimer : DbObject<WorkflowProcessTimer>
    {
        static WorkflowProcessTimer()
        {
            DbTableName = "WorkflowProcessTimer";
        }

        public WorkflowProcessTimer()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "ProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "Name"},
                new ColumnInfo {Name = "NextExecutionDateTime", Type = SqlDbType.DateTime},
                new ColumnInfo {Name = "Ignore", Type = SqlDbType.Bit},
                new ColumnInfo {Name = nameof(RootProcessId), Type = SqlDbType.UniqueIdentifier},
            });
        }

        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public string Name { get; set; }
        public DateTime NextExecutionDateTime { get; set; }
        public bool Ignore { get; set; }

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
                    Id = (Guid) value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid) value;
                    break;
                case "Name":
                    Name = (string) value;
                    break;
                case "NextExecutionDateTime":
                    NextExecutionDateTime = (DateTime) value;
                    break;
                case "Ignore":
                    Ignore = (bool) value;
                    break;
                case nameof(RootProcessId):
                    RootProcessId = (Guid) value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<int> DeleteInactiveByProcessIdAsync(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {ObjectName} WHERE [ProcessId] = @processid AND [Ignore] = 1", transaction, pProcessId).ConfigureAwait(false);
        }

        public static async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId, List<string> timersIgnoreList = null, SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<SqlParameter> {pProcessId};
                int cnt = 0;
                foreach (string timer in timersIgnoreList)
                {
                    string parameterName = $"ignore{cnt}";
                    parameters.Add($"@{parameterName}");
                    sqlParameters.Add(new SqlParameter(parameterName, SqlDbType.NVarChar) {Value = timer});
                    cnt++;
                }

                string commandText = $"DELETE FROM {ObjectName} WHERE [ProcessId] = @processid AND [Name] NOT IN ({String.Join(",", parameters)})";

                try
                {
                    return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, sqlParameters.ToArray()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw ex.RethrowAllowedIfRetrievable();
                }
            }

            try
            {
                return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE [ProcessId] = @processid", transaction, pProcessId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.RethrowAllowedIfRetrievable();
            }
        }

        public static async Task<WorkflowProcessTimer> SelectByProcessIdAndNameAsync(SqlConnection connection, Guid processId, string name)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [ProcessId] = @processid AND [Name] = @name";

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            var p2 = new SqlParameter("name", SqlDbType.NVarChar) {Value = name};

            return (await SelectAsync(connection, selectText, p1, p2).ConfigureAwait(false)).FirstOrDefault();
        }

        public static async Task<IEnumerable<WorkflowProcessTimer>> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [ProcessId] = @processid";

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<WorkflowProcessTimer>> SelectActiveByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [ProcessId] = @processid AND [Ignore] = 0";

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

        public static async Task<int> SetTimerIgnoreAsync(SqlConnection connection, Guid timerId)
        {
            string command = $"UPDATE {ObjectName} SET [Ignore] = 1 WHERE [Id] = @timerid AND [Ignore] = 0";
            
            var p1 = new SqlParameter("timerid", SqlDbType.UniqueIdentifier) { Value = timerId };
            
            return await ExecuteCommandNonQueryAsync(connection, command, p1).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessTimer[]> GetTopTimersToExecuteAsync(SqlConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT TOP {top} * FROM {ObjectName}" +
                                "WHERE [Ignore] = 0 AND [NextExecutionDateTime] <= @currentTime " +
                                "ORDER BY [NextExecutionDateTime]";

            var p1 = new SqlParameter("currentTime", SqlDbType.DateTime) { Value = now };

            return await SelectAsync(connection, selectText, p1).ConfigureAwait(false);
        }

#if !NETCOREAPP || NETCORE2
        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(Guid));
            dt.Columns.Add("ProcessId", typeof(Guid));
            dt.Columns.Add("RootProcessId", typeof(Guid));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("NextExecutionDateTime", typeof(DateTime));
            dt.Columns.Add("Ignore", typeof(bool));
            return dt;
        }
#endif
    }
}
