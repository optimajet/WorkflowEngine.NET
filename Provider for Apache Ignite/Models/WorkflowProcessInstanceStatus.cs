using System;
using System.Data;
using System.Data.SqlClient;
using Apache.Ignite.Core.Cache.Configuration;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.Ignite
{
    public class WorkflowProcessInstanceStatus : DbObject<WorkflowProcessInstanceStatus>
    {
        static WorkflowProcessInstanceStatus()
        {
            DbColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = typeof(Guid)},
                new ColumnInfo {Name = "Status", Type = typeof(byte)}
            });
        }

        [QuerySqlField]
        public Guid Id { get; set; }
        [QuerySqlField]
        public byte Status { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "Status":
                    return Status;
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
                case "Status":
                    Status = (byte)value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }
    }


}