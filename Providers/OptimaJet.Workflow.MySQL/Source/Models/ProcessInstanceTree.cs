using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.MySQL
{
    public class ProcessInstanceTree : DbObject<ProcessInstanceTreeItem>
    {
        public ProcessInstanceTree(int commandTimeout) : base("virtual", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.ParentProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.RootProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.StartingTransition)},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.SubprocessName)},
            });
        }

        public async Task<List<IProcessInstanceTreeItem>> GetProcessTreeItemsByRootProcessId(MySqlConnection connection, Guid rootProcessId)
        {
            var workflowProcessInstance = new WorkflowProcessInstance(CommandTimeout);

            var builder = new StringBuilder();
            builder.Append("SELECT ");
            builder.Append("`");
            builder.Append(nameof(ProcessInstanceTreeItem.Id));
            builder.Append("`, `");
            builder.Append(nameof(ProcessInstanceTreeItem.ParentProcessId));
            builder.Append("`, `");
            builder.Append(nameof(ProcessInstanceTreeItem.RootProcessId));
            builder.Append("`, `");
            builder.Append(nameof(ProcessInstanceTreeItem.StartingTransition));
            builder.Append("`, `");
            builder.Append(nameof(ProcessInstanceTreeItem.SubprocessName));
            builder.Append("` ");
            builder.Append("FROM ");
            builder.Append(workflowProcessInstance.DbTableName);
            builder.Append(" WHERE `");
            builder.Append(nameof(ProcessInstanceTreeItem.RootProcessId));
            builder.Append("` = @rootProcessId");

            return (await SelectAsync(connection, builder.ToString(),
                    new MySqlParameter("rootProcessId", MySqlDbType.Binary) {Value = rootProcessId.ToByteArray()})
                .ConfigureAwait(false)).Cast<IProcessInstanceTreeItem>().ToList();
        }
    }
}
