
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessInstance: DbObject<ProcessInstanceEntity>
    {
        public WorkflowProcessInstance(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessInstance", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.Id), IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.ActivityName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.IsDeterminingParametersChanged), Type = OracleDbType.Byte},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivity)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivityForDirect)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousActivityForReverse)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousState)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousStateForDirect)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.PreviousStateForReverse)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.SchemeId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.StateName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.ParentProcessId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.RootProcessId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.TenantId), Size=1024},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.StartingTransition)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.SubprocessName)},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.CreationDate), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.LastTransitionDate), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(ProcessInstanceEntity.CalendarName)}
            });
        }

        public async Task<ProcessInstanceEntity[]> GetInstances(OracleConnection connection, IEnumerable<Guid> ids)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE ID IN ({String.Join(",", ids.Select(x => $"HEXTORAW('{BitConverter.ToString(x.ToByteArray()).Replace("-", String.Empty)}')"))})";
            
            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }
    }
}
