using System;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.Ignite
{
    public class WorkflowScheme : DbObject<WorkflowScheme>
    {  
        static WorkflowScheme()
        {
            DbColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Code", IsKey = true},
                new ColumnInfo {Name = "Scheme"}
            });
        }

        public string Code { get; set; }
        public string Scheme { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
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