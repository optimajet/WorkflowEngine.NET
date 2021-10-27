using System;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public sealed class WorkflowGlobalParameter : DbObject<WorkflowGlobalParameter>
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
                new ColumnInfo {Name = "Id", IsKey = true, Type = OracleDbType.Raw},
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

        public static async Task<WorkflowGlobalParameter[]> SelectByTypeAndNameAsync(OracleConnection connection, string type, string name = null)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE Type = :type";

            if (!String.IsNullOrEmpty(name))
            {
                selectText = selectText + " AND Name = :name";
            }

            var p = new OracleParameter("type", OracleDbType.NVarchar2) { Value = type };

            if (String.IsNullOrEmpty(name))
            {
                return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
            }

            var p1 = new OracleParameter("name", OracleDbType.NVarchar2) { Value = name };

            return await SelectAsync(connection, selectText, p, p1).ConfigureAwait(false);
        }

        public static async Task<int> DeleteByTypeAndNameAsync(OracleConnection connection, string type, string name = null)
        {
            string selectText = $"DELETE FROM {ObjectName}  WHERE Type = :type";

            if (!String.IsNullOrEmpty(name))
            {
                selectText = selectText + " AND Name = :name";
            }

            var p = new OracleParameter("type", OracleDbType.NVarchar2) { Value = type };

            if (String.IsNullOrEmpty(name))
            {
                return await ExecuteCommandNonQueryAsync(connection, selectText, p).ConfigureAwait(false);
            }

            var p1 = new OracleParameter("name", OracleDbType.NVarchar2) { Value = name };

            return await ExecuteCommandNonQueryAsync(connection, selectText, p, p1).ConfigureAwait(false);
        }

   
    }
}
