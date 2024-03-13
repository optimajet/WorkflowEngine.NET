using System.Data;
using System.Text;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.SQLite
{
    public class ProcessInstanceTree : DbObject<ProcessInstanceTreeItem>
    {
        public ProcessInstanceTree(string schemaName, int commandTimeout) : base(schemaName, "virtual", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.ParentProcessId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.RootProcessId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.StartingTransition)},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.SubprocessName)},
            });
        }

        public async Task<List<IProcessInstanceTreeItem>> GetProcessTreeItemsByRootProcessId(SqliteConnection connection,
            Guid rootProcessId)
        {
            var workflowProcessInstance = new WorkflowProcessInstance(SchemaName, CommandTimeout);
            
            var builder = new StringBuilder();
            builder.Append("SELECT ");
            builder.Append("");
            builder.Append(nameof(ProcessInstanceTreeItem.Id));
            builder.Append(", ");
            builder.Append(nameof(ProcessInstanceTreeItem.ParentProcessId));
            builder.Append(", ");
            builder.Append(nameof(ProcessInstanceTreeItem.RootProcessId));
            builder.Append(", ");
            builder.Append(nameof(ProcessInstanceTreeItem.StartingTransition));
            builder.Append(", ");
            builder.Append(nameof(ProcessInstanceTreeItem.SubprocessName));
            builder.Append(" ");
            builder.Append("FROM ");
            builder.Append(workflowProcessInstance.ObjectName);
            builder.Append(" WHERE ");
            builder.Append(nameof(ProcessInstanceTreeItem.RootProcessId));
            builder.Append(" = @rootProcessId");

            var rootProcessIdParameter = new SqliteParameter("rootProcessId", DbType.String)
            {
                Value = ToDbValue(rootProcessId, DbType.Guid)
            };
            return (await SelectAsync(connection, builder.ToString(), rootProcessIdParameter)
                .ConfigureAwait(false)).Cast<IProcessInstanceTreeItem>().ToList();
        }
    }
}
