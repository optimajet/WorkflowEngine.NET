using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    public class WorkflowApprovalHistory : DbObject<ApprovalHistoryEntity>
    {
        public WorkflowApprovalHistory(string schemaName, int commandTimeout) : base(schemaName, "WorkflowApprovalHistory", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.ProcessId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.IdentityId)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.AllowedTo)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.TransitionTime), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Sort), Type = DbType.Int32},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.InitialState)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.DestinationState)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.TriggerName)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Commentary)}
            });
        }

        public static ApprovalHistoryEntity ToDB(ApprovalHistoryItem historyItem)
        {
            return new ApprovalHistoryEntity()
            {
                Id = historyItem.Id,
                ProcessId = historyItem.ProcessId,
                IdentityId = historyItem.IdentityId,
                AllowedTo = HelperParser.Join(",", historyItem.AllowedTo),
                TransitionTime = historyItem.TransitionTime,
                Sort = historyItem.Sort,
                InitialState = historyItem.InitialState,
                DestinationState = historyItem.DestinationState,
                TriggerName = historyItem.TriggerName,
                Commentary = historyItem.Commentary,
            };
        }

        public static ApprovalHistoryItem FromDB(ApprovalHistoryEntity historyEntity)
        {
            return new ApprovalHistoryItem()
            {
                Id = historyEntity.Id,
                ProcessId = historyEntity.ProcessId,
                IdentityId = historyEntity.IdentityId,
                AllowedTo = HelperParser.SplitWithTrim(historyEntity.AllowedTo, ","),
                TransitionTime = historyEntity.TransitionTime,
                Sort = historyEntity.Sort,
                InitialState = historyEntity.InitialState,
                DestinationState = historyEntity.DestinationState,
                TriggerName = historyEntity.TriggerName,
                Commentary = historyEntity.Commentary,
            };
        }

        public async Task<List<OutboxItem>> SelectOutboxByIdentityIdAsync(SqliteConnection connection, string identityId,
            Paging paging = null)
        {
            string paramName = "_" + nameof(identityId);
            string selectText = $"SELECT  a.{nameof(ApprovalHistoryEntity.ProcessId)}, a.ApprovalCount, a.FirstApprovalTime, a.LastApprovalTime," +

                                        $" (SELECT {nameof(ApprovalHistoryEntity.TriggerName)} FROM {ObjectName} c" +
                                        $" WHERE c.{nameof(ApprovalHistoryEntity.ProcessId)} = a.{nameof(ApprovalHistoryEntity.ProcessId)} " +
                                        $" and c.{nameof(ApprovalHistoryEntity.IdentityId)} =  @{paramName}" +
                                        $" ORDER BY c.{nameof(ApprovalHistoryEntity.TransitionTime)} DESC limit 1) as LastApproval" +

                                $" FROM " +
                                        $" (SELECT {nameof(ApprovalHistoryEntity.ProcessId)}," +
                                        $" Count({nameof(ApprovalHistoryEntity.Id)}) as ApprovalCount," +
                                        $" MIN({nameof(ApprovalHistoryEntity.TransitionTime)}) as FirstApprovalTime," +
                                        $" MAX({nameof(ApprovalHistoryEntity.TransitionTime)}) as LastApprovalTime" +
                                        $" FROM {ObjectName}" +
                                        $" WHERE {nameof(ApprovalHistoryEntity.IdentityId)} =  @{paramName}" +
                                        $" Group by {nameof(ApprovalHistoryEntity.ProcessId)}) a" +

                                $" Order By LastApprovalTime Desc ";

            if (paging != null)
            {
                selectText += $" LIMIT {paging.PageSize} OFFSET {paging.SkipCount()}";
            }

            var param = new SqliteParameter(paramName, DbType.String) {Value = identityId};
            return (await SelectAsDictionaryAsync(connection, selectText, param).ConfigureAwait(false))
                .Select(OutboxItemFromDictionary).ToList();
        }

        public async Task<int> GetOutboxCountByIdentityIdAsync(SqliteConnection connection, string identityId)
        {
            string paramName = "_" + nameof(identityId);
            string selectText = $"SELECT  COUNT(a.ProcessId)" +
                                $" FROM " +
                                $" (SELECT ProcessId" +
                                $" FROM {ObjectName}" +
                                $" WHERE IdentityId =  @{paramName}" +
                                $" Group by ProcessId) a";

            var param = new SqliteParameter(paramName, DbType.String) {Value = identityId};

            object result = await ExecuteCommandScalarAsync(connection, selectText, param).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        private static OutboxItem OutboxItemFromDictionary(Dictionary<string, object> fields)
        {
            var item = new OutboxItem();
            foreach (KeyValuePair<string, object> field in fields)
            {
                switch (field.Key)
                {
                    case nameof(item.ProcessId):
                        item.ProcessId = (Guid) FromDbValue(field.Value, DbType.Guid);
                        break;
                    case nameof(item.ApprovalCount):
                        item.ApprovalCount = Convert.ToInt32(field.Value);
                        break;
                    case nameof(item.FirstApprovalTime):
                        item.FirstApprovalTime = (DateTime) FromDbValue(field.Value, DbType.DateTime2);
                        break;
                    case nameof(item.LastApprovalTime):
                        item.LastApprovalTime = (DateTime) FromDbValue(field.Value, DbType.DateTime2);
                        break;
                    case nameof(item.LastApproval):
                        item.LastApproval = field.Value as string;
                        break;
                }
            }

            return item;
        }

        public async Task<int> DeleteByProcessIdAsync(SqliteConnection connection, Guid processId,
            SqliteTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<List<ApprovalHistoryEntity>> SelectByProcessIdAsync(SqliteConnection connection, Guid processId)
        {
            return (await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false)).ToList();
        }
    }
}
