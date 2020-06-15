using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.PostgreSQL
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
                new ColumnInfo {Name = "Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = "ParentProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = "RootProcessId", Type = NpgsqlDbType.Uuid},
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
                    return Id;
                case "ParentProcessId":
                    return ParentProcessId;
                case "RootProcessId":
                    return RootProcessId;
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
                    Id = (Guid) value;
                    break;
                case "ParentProcessId":
                    ParentProcessId = value as Guid?;
                    break;
                case "RootProcessId":
                    RootProcessId = (Guid) value;
                    break;
                case "StartingTransition":
                    StartingTransition = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        
        public static async Task<List<IProcessInstanceTreeItem>> GetProcessTreeItemsByRootProcessId(NpgsqlConnection connection, Guid rootProcessId)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT ");
            builder.Append($"\"{nameof(Id)}\", \"{nameof(ParentProcessId)}\", \"{nameof(RootProcessId)}\", \"{nameof(StartingTransition)}\" ");
            builder.Append($"FROM {WorkflowProcessInstance.ObjectName} ");
            builder.Append($"WHERE \"{nameof(RootProcessId)}\" = @rootProcessId");

            return (await SelectAsync(connection, builder.ToString(), new NpgsqlParameter("rootProcessId", NpgsqlDbType.Uuid) {Value = rootProcessId})
                .ConfigureAwait(false)).Cast<IProcessInstanceTreeItem>().ToList();
        }
    }
}
