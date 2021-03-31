using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task<WorkflowProcessInstancePersistence[]> SelectByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE ProcessId = :processid";
            return await SelectAsync(connection, selectText,
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input)).ConfigureAwait(false);
        }
        public static async Task<WorkflowProcessInstancePersistence> SelectByNameAsync(OracleConnection connection, Guid processId, string parameterName)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE ProcessId = :processid AND ParameterName = :parameterName";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input),
                new OracleParameter("parameterName", OracleDbType.NVarchar2, parameterName, ParameterDirection.Input)
            };
            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public static async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {ObjectName} WHERE PROCESSID = :processid",
                new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input)
            ).ConfigureAwait(false);
        }

        public static async Task<int> DeleteByNameAsync(OracleConnection connection, Guid processId, string parameterName)
        {
            List<OracleParameter> parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("processid", OracleDbType.Raw, processId.ToByteArray(), ParameterDirection.Input));
            parameters.Add(new OracleParameter("parameterName", OracleDbType.NVarchar2, parameterName, ParameterDirection.Input));

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {ObjectName} WHERE PROCESSID = :processid",
                parameters.ToArray()).ConfigureAwait(false);
        }
    }
}
