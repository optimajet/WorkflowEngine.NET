using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessInstancePersistence : DbObject<WorkflowProcessInstancePersistence>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }

        static WorkflowProcessInstancePersistence()
        {
            DbTableName = "WorkflowProcessInstanceP";
        }

        public WorkflowProcessInstancePersistence()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = "ProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "ParameterName"},
                new ColumnInfo {Name = "Value", Type = OracleDbType.Clob}
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
                    Id = new Guid((byte[]) value);
                    break;
                case "ProcessId":
                    ProcessId = new Guid((byte[]) value);
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

        public static WorkflowProcessInstancePersistence[] SelectByProcessId(OracleConnection connection, Guid processId)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE ProcessId = :processid", ObjectName);
            return Select(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input));
        }

        public static int DeleteByProcessId(OracleConnection connection, Guid processId)
        {
            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE PROCESSID = :processid", ObjectName),
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input)
                );
        }
    }
}