using System;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessTransitionHistory : DbObject<ProcessTransitionHistoryEntity>
    {
        public WorkflowProcessTransitionHistory(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessTransitionHistory", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ActorIdentityId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ExecutorIdentityId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ActorName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ExecutorName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.FromActivityName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.FromStateName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.IsFinalised), Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ProcessId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ToActivityName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ToStateName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionClassifier)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionTime), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TriggerName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.StartTransitionTime), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionDuration), Type = NpgsqlDbType.Bigint},
            });
        }

        public async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<ProcessTransitionHistoryEntity[]> SelectByProcessIdAsync(NpgsqlConnection connection, Guid processId,
            Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ProcessId, processId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }

        public async Task<ProcessTransitionHistoryEntity[]> SelectByIdentityIdAsync(NpgsqlConnection connection, string identityId,
            Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ExecutorIdentityId, identityId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }
    }
}
