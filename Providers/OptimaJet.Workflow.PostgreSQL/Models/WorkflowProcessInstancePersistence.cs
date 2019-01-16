using System;
using Npgsql;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessInstancePersistence : DbObject<WorkflowProcessInstancePersistence>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }

        static WorkflowProcessInstancePersistence()
        {
            DbTableName = "WorkflowProcessInstancePersistence";
        }

        public WorkflowProcessInstancePersistence()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="ProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="ParameterName"},
                new ColumnInfo {Name="Value", Type = NpgsqlDbType.Text }
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

        public static WorkflowProcessInstancePersistence[] SelectByProcessId(NpgsqlConnection connection, Guid processId)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE \"ProcessId\" = @processid", ObjectName);
            var p = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };
            return Select(connection, selectText, p);
        }

        public static int DeleteByProcessId(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction)
        {
            var p = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE \"ProcessId\" = @processid", ObjectName), transaction, p);
        }
    }
}