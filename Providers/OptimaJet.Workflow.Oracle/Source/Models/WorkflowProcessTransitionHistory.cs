using System;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Persistence;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessTransitionHistory : DbObject<ProcessTransitionHistoryEntity>
    {
        public WorkflowProcessTransitionHistory(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessTransitionH", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.Id), IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ActorIdentityId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ExecutorIdentityId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ActorName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ExecutorName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.FromActivityName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.FromStateName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.IsFinalised), Type = OracleDbType.Byte},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ProcessId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ToActivityName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ToStateName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionClassifier)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionTime), Type = OracleDbType.Date},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TriggerName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.StartTransitionTime), Type = OracleDbType.Date},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionDuration), Type = OracleDbType.Int64},
            });
        }

        public async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId, OracleTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<ProcessTransitionHistoryEntity[]> SelectByProcessIdAsync(OracleConnection connection, Guid processId,
            Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ProcessId, processId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }

        public async Task<ProcessTransitionHistoryEntity[]> SelectByIdentityIdAsync(OracleConnection connection, string identityId,
            Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ExecutorIdentityId, identityId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }
    }
}
