using System;
using NpgsqlTypes;

namespace OptimaJet.Workflow.PostgreSQL.Models
{
    public class WorkflowScheme : DbObject<WorkflowScheme>
    {
        public string Code { get; set; }
        public string Scheme { get; set; }

        public WorkflowScheme()
        {
            db_TableName = "WorkflowScheme";
            db_Columns.AddRange(new ColumnInfo[]{
                new ColumnInfo(){Name="Code", IsKey = true},
                new ColumnInfo(){Name="Scheme", Type = NpgsqlDbType.Text}
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
