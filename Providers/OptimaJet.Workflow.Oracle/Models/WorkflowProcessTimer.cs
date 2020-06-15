using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static int DeleteInactiveByProcessId(OracleConnection connection, Guid processId)
        {
            var pProcessId = new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input);

            return ExecuteCommand(connection, string.Format("DELETE FROM {0} WHERE PROCESSID = :processid AND IGNORE = 1", ObjectName), pProcessId);
        }

        public static int DeleteByProcessId(OracleConnection connection, Guid processId, List<string> timersIgnoreList = null)
        {
            var pProcessId = new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input);

            if (timersIgnoreList != null && timersIgnoreList.Any())
            {
                var parameters = new List<string>();
                var sqlParameters = new List<OracleParameter>() {pProcessId};
                var cnt = 0;
                foreach (var timer in timersIgnoreList)
                {
                    var parameterName = string.Format("ignore{0}", cnt);
                    parameters.Add(string.Format(":{0}", parameterName));
                    sqlParameters.Add(new OracleParameter(parameterName, OracleDbType.NVarchar2, timer, ParameterDirection.Input));
                    cnt++;
                }

                var commandText = string.Format("DELETE FROM {0} WHERE PROCESSID = :processid AND NAME NOT IN ({1})", ObjectName, string.Join(",", parameters));

                return ExecuteCommand(connection, commandText, sqlParameters.ToArray());
            }

            return ExecuteCommand(connection, string.Format("DELETE FROM {0} WHERE PROCESSID = :processid", ObjectName), pProcessId);

        }

        public static WorkflowProcessTimer SelectByProcessIdAndName(OracleConnection connection, Guid processId, string name)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE PROCESSID = :processid AND NAME = :name", ObjectName);
    
            return Select(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input),
                new OracleParameter("name", OracleDbType.NVarchar2, name, ParameterDirection.Input))
                .FirstOrDefault();
        }

        public static IEnumerable<WorkflowProcessTimer> SelectByProcessId(OracleConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE PROCESSID = :processid", ObjectName);

            return Select(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input));
        }

        public static IEnumerable<WorkflowProcessTimer> SelectActiveByProcessId(OracleConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE PROCESSID = :processid AND IGNORE = 0", ObjectName);

            return Select(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input));
        }

        public static int ClearTimerIgnore(OracleConnection connection, Guid timerId)
        {
            var command = string.Format("UPDATE {0} SET IGNORE = 0 WHERE ID = :timerid", ObjectName);
            var p1 = new OracleParameter("timerid", OracleDbType.Raw, timerId.ToByteArray(), ParameterDirection.Input);
            return ExecuteCommand(connection, command, p1);
        }

        public static int SetTimerIgnore(OracleConnection connection, Guid timerId)
        {
            var command = string.Format("UPDATE {0} SET IGNORE = 1 WHERE ID = :timerid AND IGNORE = 0", ObjectName);
            var p1 = new OracleParameter("timerid", OracleDbType.Raw, timerId.ToByteArray(), ParameterDirection.Input);
            return ExecuteCommand(connection, command, p1);
        }

        public static WorkflowProcessTimer GetCloseExecutionTimer(OracleConnection connection)
        {
            string selectText = string.Format("SELECT * FROM ( SELECT * FROM {0}  WHERE IGNORE = 0 ORDER BY NextExecutionDateTime) WHERE ROWNUM = 1", ObjectName);
            return Select(connection, selectText).FirstOrDefault();
        }

        public static WorkflowProcessTimer[] GetTimersToExecute(OracleConnection connection, DateTime now)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE IGNORE = 0 AND NextExecutionDateTime <= :currentTime", ObjectName);
            return Select(connection, selectText,
                new OracleParameter("currentTime", OracleDbType.Date, now, ParameterDirection.Input));
        }

        public static WorkflowProcessTimer[] GetTopTimersToExecute(OracleConnection connection, int top, DateTime now)
        {
            string selectText = $"SELECT * FROM (SELECT * FROM {ObjectName} " +
                                "WHERE IGNORE = 0 AND NextExecutionDateTime <= :currentTime " +
                                "ORDER BY NextExecutionDateTime) WHERE ROWNUM <= :rowsCount";

            return Select(connection, selectText,
                new OracleParameter("currentTime", OracleDbType.Date, now, ParameterDirection.Input),
                new OracleParameter("rowsCount", OracleDbType.Int32, top, ParameterDirection.Input)
            );
        }

        public static int SetIgnore(OracleConnection connection, WorkflowProcessTimer[] timers)
        {
            if (timers.Length == 0)
                return 0;
            var result = 0;
            var skip = 0;
            var take = 1000;

            while (skip < timers.Length)
            {

                var parameters = new List<string>();
                var sqlParameters = new List<OracleParameter>();
                var cnt = 0;

                foreach (var timer in timers.Skip(skip).Take(take))
                {
                    var parameterName = string.Format("timer{0}", cnt);
                    parameters.Add(string.Format(":{0}", parameterName));
                    sqlParameters.Add(new OracleParameter(parameterName, OracleDbType.Raw, timer.Id.ToByteArray(), ParameterDirection.Input));
                    cnt++;
                }

                result = result + ExecuteCommand(connection,
                             string.Format("UPDATE {0} SET IGNORE = 1 WHERE ID IN ({1})", ObjectName, string.Join(",", parameters)),
                             sqlParameters.ToArray());

                skip = skip + take;
            }

            return result;
        }
    }
}
