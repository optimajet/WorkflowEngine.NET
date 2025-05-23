using System;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessTransitionHistory : DbObject<ProcessTransitionHistoryEntity>
    {
        public WorkflowProcessTransitionHistory(int commandTimeout) : base("workflowprocesstransitionhistory", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ActorIdentityId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ExecutorIdentityId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ActorName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ExecutorName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.FromActivityName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.FromStateName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.IsFinalised), Type = MySqlDbType.Bit},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ToActivityName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ToStateName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionClassifier)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionTime), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TriggerName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.StartTransitionTime), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionDuration), Type = MySqlDbType.Int64},
            });
        }

        public async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId, MySqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<ProcessTransitionHistoryEntity[]> SelectByProcessIdAsync(MySqlConnection connection, Guid processId,
            Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ProcessId, processId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }

        public async Task<ProcessTransitionHistoryEntity[]> SelectByIdentityIdAsync(MySqlConnection connection, string identityId,
            Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ExecutorIdentityId, identityId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }
    }
}
