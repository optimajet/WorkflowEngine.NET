using System;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowGlobalParameter : DbObject<WorkflowGlobalParameter>
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        static WorkflowGlobalParameter()
        {
            DbTableName = "workflowglobalparameter";
        }
        
        public WorkflowGlobalParameter()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = "Type"},
                new ColumnInfo {Name = "Name"},
                new ColumnInfo {Name = "Value"}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "Type":
                    return Type;
                case "Name":
                    return Name;
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
                case "Type":
                    Type = value as string;
                    break;
                case "Name":
                    Name = value as string;
                    break;
                case "Value":
                    Value = value as string;
                    break;
               default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static WorkflowGlobalParameter[] SelectByTypeAndName(MySqlConnection connection, string type, string name = null)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE `Type` = @type", DbTableName);

            if (!string.IsNullOrEmpty(name))
                selectText = selectText + " AND `Name` = @name";

            var p = new MySqlParameter("type", MySqlDbType.VarString) {Value = type};

            if (string.IsNullOrEmpty(name))
                return Select(connection, selectText, p);

            var p1 = new MySqlParameter("name", MySqlDbType.VarString) { Value = name };

            return Select(connection, selectText, p, p1);
        }

        public static int DeleteByTypeAndName(MySqlConnection connection, string type, string name = null)
        {
            string selectText = string.Format("DELETE FROM {0}  WHERE `Type` = @type", DbTableName);

            if (!string.IsNullOrEmpty(name))
                selectText = selectText + " AND `Name` = @name";

            var p = new MySqlParameter("type", MySqlDbType.VarString) { Value = type };

            if (string.IsNullOrEmpty(name))
                return ExecuteCommand(connection, selectText, p);

            var p1 = new MySqlParameter("name", MySqlDbType.VarString) { Value = name };

            return ExecuteCommand(connection, selectText, p, p1);
        }
    }
}