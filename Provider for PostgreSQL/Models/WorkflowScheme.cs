using System;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowScheme : DbObject<WorkflowScheme>
    {
        public string Code { get; set; }
        public string Scheme { get; set; }

        static WorkflowScheme()
        {
            DbTableName = "WorkflowScheme";
        }

        public WorkflowScheme()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Code", IsKey = true},
                new ColumnInfo {Name="Scheme", Type = NpgsqlDbType.Text}
            });
        }

        public override object GetValue(string key)
        {
            switch(key)
            {
                case "Code":
                    return Code;
                case "Scheme":
                    return Scheme;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Code":
                    Code = value as string;
                    break;
                case "Scheme":
                    Scheme = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }
    }
}
