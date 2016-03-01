using System;
using System.Data.SqlClient;
using System.Data;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessInstancePersistence : DbObject<WorkflowProcessInstancePersistence>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }

        private const string TableName = "WorkflowProcessInstancePersistence";

        public WorkflowProcessInstancePersistence()
        {
            DbTableName = "WorkflowProcessInstancePersistence";
            DbColumns.AddRange(new[]{
                new ColumnInfo(){Name="Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo(){Name="ProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo(){Name="ParameterName"},
                new ColumnInfo(){Name="Value", Type = SqlDbType.NVarChar, Size = -1}
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
                    Id = (Guid)value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid)value;
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
            string selectText = string.Format("SELECT * FROM [{0}]  WHERE [ProcessId] = @processid", TableName);
            var p = new SqlParameter("processId", SqlDbType.UniqueIdentifier) {Value = processId};
            return Select(connection, selectText, p);
        }

        public static int DeleteByProcessId(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            var p = new SqlParameter("processId", SqlDbType.UniqueIdentifier) {Value = processId};

            return ExecuteCommand(connection,
                string.Format("DELETE FROM [{0}] WHERE [ProcessId] = @processid", TableName), transaction, p);
        }
    }
}