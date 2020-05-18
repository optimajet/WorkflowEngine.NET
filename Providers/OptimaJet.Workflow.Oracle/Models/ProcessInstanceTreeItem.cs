using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Persistence;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.Oracle
{
    public class ProcessInstanceTreeItem : DbObject<ProcessInstanceTreeItem>, IProcessInstanceTreeItem
    {
        static ProcessInstanceTreeItem()
        {
            DbTableName = "virtual"; //this entity is combination of WorkflowProcessInstance and WorkflowProcessScheme
        }

        public ProcessInstanceTreeItem()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = "ParentProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "RootProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "StartingTransition"}
            });
        }

        public Guid Id { get; set; }
        public Guid? ParentProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public string StartingTransition { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "ParentProcessId":
                    return ParentProcessId?.ToByteArray();
                case "RootProcessId":
                    return RootProcessId.ToByteArray();
                case "StartingTransition":
                    return StartingTransition;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                {
                    Id = (value is byte[] bytes) ? new Guid(bytes) : throw new Exception($"DB mapping error. {nameof(Id)} can't be null");
                    break;
                }
                case "ParentProcessId":
                {
                    ParentProcessId = (value is byte[] bytes) ? new Guid(bytes) : (Guid?)null;
                    break;
                }
                case "RootProcessId":
                {
                    RootProcessId = (value is byte[] bytes) ? new Guid(bytes) : throw new Exception($"DB mapping error. {nameof(RootProcessId)} can't be null");
                    break;
                }
                case "StartingTransition":
                    StartingTransition = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        
        public static async Task<List<IProcessInstanceTreeItem>> GetProcessTreeItemsByRootProcessId(OracleConnection connection, Guid rootProcessId)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT ");
            builder.Append($"{nameof(Id)}, {nameof(ParentProcessId)}, {nameof(RootProcessId)}, {nameof(StartingTransition)} ");
            builder.Append($"FROM {WorkflowProcessInstance.ObjectName} ");
            builder.Append($"WHERE {nameof(RootProcessId)} = :rootProcessId");

            var rootProcessIdParameter = new OracleParameter("rootProcessId", OracleDbType.Raw, rootProcessId.ToByteArray(), ParameterDirection.Input);

            return (await SelectAsync(connection, builder.ToString(), rootProcessIdParameter)
                .ConfigureAwait(false)).Cast<IProcessInstanceTreeItem>().ToList();
        }
    }
}
