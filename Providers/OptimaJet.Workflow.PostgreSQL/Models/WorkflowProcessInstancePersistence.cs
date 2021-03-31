using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task<WorkflowProcessInstancePersistence[]> SelectByProcessIdAsync(NpgsqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"ProcessId\" = @processid";
            var p = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessInstancePersistence> SelectByNameAsync(NpgsqlConnection connection, Guid processId,  string parameterName)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"ProcessId\" = @processid AND \"ParameterName\" = @parameterName";

            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId}, 
                new NpgsqlParameter("parameterName", NpgsqlDbType.Text) {Value = parameterName}
            };
            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public static async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction = null)
        {
            var p = new NpgsqlParameter("processid", NpgsqlDbType.Uuid) { Value = processId };

            return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE \"ProcessId\" = @processid", transaction, p).ConfigureAwait(false);
        }

        public static async Task<int> DeleteByNameAsync(NpgsqlConnection connection, Guid processId, string parameterName, NpgsqlTransaction transaction = null)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("processid", NpgsqlDbType.Uuid) {Value = processId},
                new NpgsqlParameter("parameterName", NpgsqlDbType.Text) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {ObjectName} WHERE \"ProcessId\" = @processid AND \"ParameterName\" = @parameterName", transaction, parameters.ToArray()).ConfigureAwait(false);
        }
    }
}
