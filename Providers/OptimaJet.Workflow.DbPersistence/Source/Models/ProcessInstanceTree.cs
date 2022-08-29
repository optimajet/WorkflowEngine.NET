using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class ProcessInstanceTree : DbObject<ProcessInstanceTreeItem>
    {
        public ProcessInstanceTree(string schemaName, int commandTimeout) : base(schemaName, "virtual", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.ParentProcessId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.RootProcessId), Type = SqlDbType.UniqueIdentifier}, 
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.StartingTransition)},
                new ColumnInfo {Name = nameof(ProcessInstanceTreeItem.SubprocessName)},
            });
        }

        public async Task<List<IProcessInstanceTreeItem>> GetProcessTreeItemsByRootProcessIdAsync(SqlConnection connection,
            Guid rootProcessId)
        {
            var workflowProcessInstance = new WorkflowProcessInstance(SchemaName, CommandTimeout);
            
            var builder = new StringBuilder();
            builder.Append("SELECT ");
            builder.Append("[");
            builder.Append(nameof(ProcessInstanceTreeItem.Id));
            builder.Append("], [");
            builder.Append(nameof(ProcessInstanceTreeItem.ParentProcessId));
            builder.Append("], [");
            builder.Append(nameof(ProcessInstanceTreeItem.RootProcessId));
            builder.Append("], [");
            builder.Append(nameof(ProcessInstanceTreeItem.StartingTransition));
            builder.Append("], [");
            builder.Append(nameof(ProcessInstanceTreeItem.SubprocessName) );
            builder.Append("] ");
            builder.Append("FROM ");
            builder.Append(workflowProcessInstance.ObjectName);
            builder.Append(" WHERE [");
            builder.Append(nameof(ProcessInstanceTreeItem.RootProcessId) );
            builder.Append("] = @rootProcessId");

            return (await SelectAsync(connection, builder.ToString(),
                    new SqlParameter("rootProcessId", SqlDbType.UniqueIdentifier) {Value = rootProcessId})
                .ConfigureAwait(false)).Cast<IProcessInstanceTreeItem>().ToList();
        }
    }
}
