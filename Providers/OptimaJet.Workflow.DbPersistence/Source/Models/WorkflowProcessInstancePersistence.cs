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

        public static async Task<WorkflowProcessInstancePersistence[]> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE [ProcessId] = @processid";
            var p = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};
            return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessInstancePersistence> SelectByNameAsync(SqlConnection connection, Guid processId, string parameterName)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE [ProcessId] = @processid AND [ParameterName] = @parameterName";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId}, 
                new SqlParameter("parameterName", SqlDbType.NVarChar) {Value = parameterName}
            };

            return (await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false)).SingleOrDefault();
        }

        public static async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            var p = new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId};

            return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE [ProcessId] = @processid", transaction, p).ConfigureAwait(false);
        }
        public static async Task<int> DeleteByNameAsync(SqlConnection connection, Guid processId,string parameterName, SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("processid", SqlDbType.UniqueIdentifier) {Value = processId}, 
                new SqlParameter("parameterName", SqlDbType.NVarChar) {Value = parameterName}
            };

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {ObjectName}  WHERE [ProcessId] = @processid AND [ParameterName] = @parameterName", transaction, parameters.ToArray()).ConfigureAwait(false);
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
