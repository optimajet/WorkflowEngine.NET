using System;
using System.Data.SqlClient;
using System.Data;
using Apache.Ignite.Core.Cache.Configuration;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Ignite
{
    public class WorkflowGlobalParameter : DbObject<WorkflowGlobalParameter>
    {
        public Guid Id { get; set; }


        [QuerySqlField]
        public string Type { get; set; }


        [QuerySqlField]
        public string Name { get; set; }

        public string Value { get; set; }
        
        static WorkflowGlobalParameter()
        {
            DbColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = typeof(Guid)},
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
    }
}