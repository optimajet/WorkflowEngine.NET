using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessInstancePersistence : DbObject<WorkflowProcessInstancePersistence>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }

        private static string _tableName = "WorkflowProcessInstancePersistence";

        public WorkflowProcessInstancePersistence()
            : base()
        {
            db_TableName = _tableName;
            db_Columns.AddRange(new ColumnInfo[]{
                new ColumnInfo(){Name="Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo(){Name="ProcessId", Type = MySqlDbType.Binary},
                new ColumnInfo(){Name="ParameterName"},
                new ColumnInfo(){Name="Value", Type = MySqlDbType.LongText }
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

        public static WorkflowProcessInstancePersistence[] SelectByProcessId(MySqlConnection connection, Guid processId)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE `ProcessId` = @processid", _tableName);
            var p = new MySqlParameter("processId", MySqlDbType.Binary);
            p.Value = processId.ToByteArray();
            return Select(connection, selectText, p);
        }

        public static int DeleteByProcessId(MySqlConnection connection, Guid processId)
        {
            var p = new MySqlParameter("processId", MySqlDbType.Binary);
            p.Value = processId.ToByteArray();

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE `ProcessId` = @processid", _tableName), p);
        }
    }
}