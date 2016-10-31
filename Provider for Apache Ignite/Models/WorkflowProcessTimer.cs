using System;
using Apache.Ignite.Core.Cache.Configuration;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.Ignite
{
    public class WorkflowProcessTimer : DbObject<WorkflowProcessTimer>
    {
        static WorkflowProcessTimer()
        {
            DbColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = typeof(Guid)},
                new ColumnInfo {Name = "ProcessId", Type = typeof(Guid)},
                new ColumnInfo {Name = "Name"},
                new ColumnInfo {Name = "NextExecutionDateTime", Type = typeof(DateTime)},
                new ColumnInfo {Name = "Ignore", Type = typeof(bool)}
            });
        }

        public Guid Id { get; set; }
        [QuerySqlField]
        public Guid ProcessId { get; set; }
        [QuerySqlField]
        public string Name { get; set; }
        [QuerySqlField]
        public DateTime NextExecutionDateTime { get; set; }
        [QuerySqlField]
        public bool Ignore { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "ProcessId":
                    return ProcessId;
                case "Name":
                    return Name;
                case "NextExecutionDateTime":
                    return NextExecutionDateTime;
                case "Ignore":
                    return Ignore;
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
                case "Name":
                    Name = (string) value;
                    break;
                case "NextExecutionDateTime":
                    NextExecutionDateTime = (DateTime) value;
                    break;
                case "Ignore":
                    Ignore = (bool) value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }
    }
}