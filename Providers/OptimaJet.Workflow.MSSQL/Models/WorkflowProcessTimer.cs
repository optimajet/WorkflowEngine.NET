using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

        public static int DeleteInactiveByProcessId(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };

            return ExecuteCommand(connection,
                String.Format("DELETE FROM {0} WHERE [ProcessId] = @processid AND [Ignore] = 1", ObjectName), transaction, pProcessId);
        }

        public static int DeleteByProcessId(SqlConnection connection, Guid processId,
            List<string> timersIgnoreList = null, SqlTransaction transaction = null)
        {
            var pProcessId = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<SqlParameter> {pProcessId};
                var cnt = 0;
                foreach (var timer in timersIgnoreList)
                {
                    var parameterName = string.Format("ignore{0}", cnt);
                    parameters.Add(string.Format("@{0}", parameterName));
                    sqlParameters.Add(new SqlParameter(parameterName, SqlDbType.NVarChar) {Value = timer});
                    cnt++;
                }

                var commandText = string.Format(
                    "DELETE FROM {0} WHERE [ProcessId] = @processid AND [Name] NOT IN ({1})",
                    ObjectName, string.Join(",", parameters));

                try
                {
                    return ExecuteCommand(connection,
                        commandText, transaction, sqlParameters.ToArray());
                }
                catch (Exception ex)
                {
                    throw ex.RethrowAllowedIfRetrievable();
                }

            }

            try
            {
                return ExecuteCommand(connection,
                    string.Format("DELETE FROM {0} WHERE [ProcessId] = @processid", ObjectName), transaction, pProcessId);
            }
            catch (Exception ex)
            {
                throw ex.RethrowAllowedIfRetrievable();
            }
        }

        public static WorkflowProcessTimer SelectByProcessIdAndName(SqlConnection connection, Guid processId, string name)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE [ProcessId] = @processid AND [Name] = @name", ObjectName);

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            var p2 = new SqlParameter("name", SqlDbType.NVarChar) {Value = name};

            return Select(connection, selectText, p1, p2).FirstOrDefault();
        }

        public static IEnumerable<WorkflowProcessTimer> SelectByProcessId(SqlConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE [ProcessId] = @processid", ObjectName);

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };

            return Select(connection, selectText, p1);
        }

        public static IEnumerable<WorkflowProcessTimer> SelectActiveByProcessId(SqlConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE [ProcessId] = @processid AND [Ignore] = 0", ObjectName);

            var p1 = new SqlParameter("processid", SqlDbType.UniqueIdentifier) { Value = processId };

            return Select(connection, selectText, p1);
        }

        public static int ClearTimerIgnore(SqlConnection connection, Guid timerId)
        {
            var command = string.Format("UPDATE {0} SET [Ignore] = 0 WHERE [Id] = @timerid", ObjectName);
            var p1 = new SqlParameter("timerid", SqlDbType.UniqueIdentifier) {Value = timerId};
            return ExecuteCommand(connection, command, p1);
        }

        public static int SetTimerIgnore(SqlConnection connection, Guid timerId)
        {
            var command = string.Format("UPDATE {0} SET [Ignore] = 1 WHERE [Id] = @timerid AND [Ignore] = 0", ObjectName);
            var p1 = new SqlParameter("timerid", SqlDbType.UniqueIdentifier) { Value = timerId };
            return ExecuteCommand(connection, command, p1);
        }

        public static WorkflowProcessTimer GetCloseExecutionTimer(SqlConnection connection)
        {
            var selectText = string.Format("SELECT TOP 1 * FROM {0}  WHERE [Ignore] = 0 ORDER BY [NextExecutionDateTime]", ObjectName);

            var parameters = new SqlParameter[] {};

            return Select(connection, selectText, parameters).FirstOrDefault();
        }

        public static WorkflowProcessTimer[] GetTimersToExecute(SqlConnection connection, DateTime now)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE [Ignore] = 0 AND [NextExecutionDateTime] <= @currentTime", ObjectName);

            var p = new SqlParameter("currentTime", SqlDbType.DateTime) {Value = now};

            return Select(connection, selectText, p);
        }

        public static WorkflowProcessTimer[] GetTopTimersToExecute(SqlConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT TOP {top} * FROM {ObjectName}" +
                                "WHERE [Ignore] = 0 AND [NextExecutionDateTime] <= @currentTime " +
                                "ORDER BY [NextExecutionDateTime]";

            var p1 = new SqlParameter("currentTime", SqlDbType.DateTime) { Value = now };

            return Select(connection, selectText, p1);
        }

        public static int SetIgnore(SqlConnection connection, WorkflowProcessTimer[] timers)
        {
            if (timers.Length == 0)
                return 0;
            var result = 0;
            var skip = 0;
            var take = 2000;

            while (skip < timers.Length)
            {

                var parameters = new List<string>();
                var sqlParameters = new List<SqlParameter>();
                var cnt = 0;

                foreach (var timer in timers.Skip(skip).Take(take))
                {
                    var parameterName = string.Format("timer{0}", cnt);
                    parameters.Add(string.Format("@{0}", parameterName));
                    sqlParameters.Add(new SqlParameter(parameterName, SqlDbType.UniqueIdentifier) {Value = timer.Id});
                    cnt++;
                }

                result = result + ExecuteCommand(connection,
                             string.Format("UPDATE {0} SET [Ignore] = 1 WHERE [Id] IN ({1})", ObjectName, string.Join(",", parameters)),
                             sqlParameters.ToArray());

                skip = skip + take;
            }

            return result;
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
