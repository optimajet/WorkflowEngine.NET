using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessInstance : DbObject<ProcessInstanceEntity>
    {
        public WorkflowProcessInstance(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessInstance", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.ActivityName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.IsDeterminingParametersChanged), Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivity)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivityForDirect)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivityForReverse)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousState)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousStateForDirect)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousStateForReverse)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.SchemeId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.StateName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.ParentProcessId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.RootProcessId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.TenantId)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.StartingTransition)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.SubprocessName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.CreationDate), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.LastTransitionDate), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.CalendarName)}
            });
        }

        public async Task<ProcessInstanceEntity[]> GetInstancesAsync(NpgsqlConnection connection, IEnumerable<Guid> ids)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE \"Id\" IN ({String.Join(",", ids.Select(x => $"'{x}'"))})";
            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }
    }
}
