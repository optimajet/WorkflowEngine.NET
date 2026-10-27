using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    /// <summary>
    /// Database object for workflow process transition history operations.
    /// </summary>
    public class WorkflowProcessTransitionHistory : DbObject<ProcessTransitionHistoryEntity>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WorkflowProcessTransitionHistory"/>.
        /// </summary>
        /// <param name="schemaName">The database schema name.</param>
        /// <param name="commandTimeout">The command timeout in seconds.</param>
        public WorkflowProcessTransitionHistory(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessTransitionHistory", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ActorIdentityId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ExecutorIdentityId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ActorName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ExecutorName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.FromActivityName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.FromStateName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.IsFinalised), Type = DbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ProcessId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TenantId)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ToActivityName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.ToStateName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionClassifier)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionTime), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TriggerName)},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.StartTransitionTime), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessTransitionHistoryEntity.TransitionDuration), Type = DbType.Int32},
            });
        }

        /// <summary>
        /// Deletes all transition history records for the specified process.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="transaction">An optional transaction.</param>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> DeleteByProcessIdAsync(SqliteConnection connection, Guid processId, SqliteTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Selects transition history records for the specified process, sorted by transition time descending.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="paging">The paging settings, or <c>null</c> to return all records.</param>
        /// <returns>An array of transition history entities.</returns>
        public async Task<ProcessTransitionHistoryEntity[]> SelectByProcessIdAsync(SqliteConnection connection, Guid processId,
            Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ProcessId, processId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects transition history records for the specified identity, sorted by transition time descending.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="identityId">The identity identifier.</param>
        /// <param name="paging">The paging settings, or <c>null</c> to return all records.</param>
        /// <returns>An array of transition history entities.</returns>
        public async Task<ProcessTransitionHistoryEntity[]> SelectByIdentityIdAsync(SqliteConnection connection, string identityId,
            Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ExecutorIdentityId, identityId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }
    }
}
