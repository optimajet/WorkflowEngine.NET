using System;
using System.Data;
using System.Data.SqlClient;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessInstancePersistence : DbObject<WorkflowProcessInstancePersistence>
    {
        static WorkflowProcessInstancePersistence()
        {
            DbTableName = "WorkflowProcessInstancePersistence";
        }

        public WorkflowProcessInstancePersistence()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "ProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "ParameterName"},
                new ColumnInfo {Name = "Value", Type = SqlDbType.NVarChar, Size = -1}
            });
        }

        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "ProcessId":
                    return ProcessId;
                case "ParameterName":
                    return ParameterName;
                case "Value":
                    return Value;
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
                case "ParameterName":
                    ParameterName = value as string;
                    break;
                case "Value":
                    Value = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static WorkflowProcessInstancePersistence[] SelectByProcessId(SqlConnection connection, Guid processId)
        {
            var selectText = string.Format("SELECT * FROM {0}  WHERE [ProcessId] = @processid", ObjectName);
            var p = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};
            return Select(connection, selectText, p);
        }

        public static int DeleteByProcessId(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            var p = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE [ProcessId] = @processid", ObjectName), transaction, p);
        }

#if !NETCOREAPP || NETCORE2
        public static DataTable ToDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(Guid));
            dt.Columns.Add("ProcessId", typeof(Guid));
            dt.Columns.Add("ParameterName", typeof(string));
            dt.Columns.Add("Value", typeof(string));
            return dt;
        }
#endif
    }
}