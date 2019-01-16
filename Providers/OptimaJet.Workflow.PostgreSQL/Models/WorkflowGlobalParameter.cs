using System;
using Npgsql;
using NpgsqlTypes;


// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowGlobalParameter : DbObject<WorkflowGlobalParameter>
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        static WorkflowGlobalParameter()
        {
            DbTableName = "WorkflowGlobalParameter";
        }

        public WorkflowGlobalParameter()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = NpgsqlDbType.Uuid},
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
                    return Id;
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
                    Id = (Guid)value;
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

        public static WorkflowGlobalParameter[] SelectByTypeAndName(NpgsqlConnection connection, string type, string name = null)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE \"Type\" = @type", ObjectName);

            if (!string.IsNullOrEmpty(name))
                selectText = selectText + " AND \"Name\" = @name";

            var p = new NpgsqlParameter("type", NpgsqlDbType.Varchar) { Value = type };

            if (string.IsNullOrEmpty(name))
                return Select(connection, selectText, p);

            var p1 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) { Value = name };

            return Select(connection, selectText, p, p1);
        }

        public static int DeleteByTypeAndName(NpgsqlConnection connection, string type, string name = null)
        {
            string selectText = string.Format("DELETE FROM {0}  WHERE \"Type\" = @type", ObjectName);

            if (!string.IsNullOrEmpty(name))
                selectText = selectText + " AND \"Name\" = @name";

            var p = new NpgsqlParameter("type", NpgsqlDbType.Varchar) { Value = type };

            if (string.IsNullOrEmpty(name))
                return ExecuteCommand(connection, selectText, p);

            var p1 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) { Value = name };

            return ExecuteCommand(connection, selectText, p, p1);
        }
    }
}