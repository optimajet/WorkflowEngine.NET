using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessInstancePersistence : DbObject<WorkflowProcessInstancePersistence>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }

        static WorkflowProcessInstancePersistence()
        {
            DbTableName = "workflowprocessinstancepersistence";
        }

        public WorkflowProcessInstancePersistence()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name="ProcessId", Type = MySqlDbType.Binary},
                new ColumnInfo {Name="ParameterName"},
                new ColumnInfo {Name="Value", Type = MySqlDbType.LongText }
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
                    Id = new Guid((byte[])value);
                    break;
                case "ProcessId":
                    ProcessId = new Guid((byte[])value);
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

        public static async Task<WorkflowProcessInstancePersistence[]> SelectByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {DbTableName}  WHERE `ProcessId` = @processid";
            var p = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }
        public static async Task<WorkflowProcessInstancePersistence> SelectByNameAsync(MySqlConnection connection, Guid processId, string parameterName)
        {
            string selectText = $"SELECT * FROM {DbTableName}  WHERE `ProcessId` = @processid AND `ParameterName` = @parameterName";

            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()},
                new MySqlParameter("parameterName", MySqlDbType.VarChar) {Value = parameterName}
            };
            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }
        public static async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId, MySqlTransaction transaction = null)
        {
            var p = new MySqlParameter("processid", MySqlDbType.Binary) { Value = processId.ToByteArray() };

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {DbTableName} WHERE `ProcessId` = @processid", transaction, p).ConfigureAwait(false);
        }
        public static async Task<int> DeleteByNameAsync(MySqlConnection connection, Guid processId, string parameterName, MySqlTransaction transaction = null)
        {
            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("processid", MySqlDbType.Binary) {Value = processId.ToByteArray()},
                new MySqlParameter("parameterName", MySqlDbType.VarChar) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {DbTableName} WHERE `ProcessId` = @processid", transaction, parameters.ToArray()).ConfigureAwait(false);
        }
    }
}
