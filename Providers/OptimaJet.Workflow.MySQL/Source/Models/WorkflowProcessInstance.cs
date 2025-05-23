using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessInstance : DbObject<ProcessInstanceEntity>
    {
        public WorkflowProcessInstance(int commandTimeout) : base("workflowprocessinstance", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.ActivityName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.IsDeterminingParametersChanged), Type = MySqlDbType.Bit},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivity)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivityForDirect)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivityForReverse)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousState)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousStateForDirect)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousStateForReverse)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.SchemeId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.StateName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.ParentProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.RootProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.TenantId), Type = MySqlDbType.VarChar, Size = 1024},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.StartingTransition)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.SubprocessName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.CreationDate), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.LastTransitionDate), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.CalendarName)}
            });
        }
        
        public async Task<ProcessInstanceEntity[]> GetProcessInstancesAsync(MySqlConnection connection, IEnumerable<Guid> ids)
        {
            string selectText =
                $"SELECT * FROM {DbTableName} WHERE `{nameof(ProcessInstanceEntity.Id)}` " + 
                $"IN ({String.Join(",", ids.Select(x => $"UUID_TO_BIN('{BitConverter.ToString(x.ToByteArray()).Replace("-", String.Empty)}')"))})";
            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }
    }
}
