using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowApprovalHistory : DbObject<ApprovalHistoryEntity>
    {
        public WorkflowApprovalHistory(int commandTimeout) : base("workflowapprovalhistory", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.ProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.IdentityId)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.AllowedTo)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.TransitionTime), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Sort), Type = MySqlDbType.Int64},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.InitialState), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.DestinationState)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.TriggerName)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Commentary)},
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

        public static ApprovalHistoryItem FromDB(ApprovalHistoryEntity historyItem)
        {
            return new ApprovalHistoryItem()
            {
                Id = historyItem.Id,
                ProcessId = historyItem.ProcessId,
                IdentityId = historyItem.IdentityId,
                AllowedTo = HelperParser.SplitWithTrim(historyItem.AllowedTo, ","),
                TransitionTime = historyItem.TransitionTime,
                Sort = historyItem.Sort,
                InitialState = historyItem.InitialState,
                DestinationState = historyItem.DestinationState,
                TriggerName = historyItem.TriggerName,
                Commentary = historyItem.Commentary,
            };
        }

        public async Task<List<OutboxItem>> SelectOutboxByIdentityIdAsync(MySqlConnection connection, string identityId,
            Paging paging = null)
        {
            string paramName = "_" + nameof(identityId);
            string selectText = $"SELECT  a.{nameof(ApprovalHistoryEntity.ProcessId)}, a.ApprovalCount, a.FirstApprovalTime, a.LastApprovalTime," +

                                        $" (SELECT `{nameof(ApprovalHistoryEntity.TriggerName)}` FROM {DbTableName} c" +
                                        $" WHERE c.{nameof(ApprovalHistoryEntity.ProcessId)} = a.{nameof(ApprovalHistoryEntity.ProcessId)} " +
                                        $" and c.{nameof(ApprovalHistoryEntity.IdentityId)} =  @{paramName}" +
                                        $" ORDER BY c.{nameof(ApprovalHistoryEntity.TransitionTime)} DESC LIMIT 1) as LastApproval " +

                                $" FROM " +
                                        $" (SELECT `{nameof(ApprovalHistoryEntity.ProcessId)}`," +
                                        $" Count(`{nameof(ApprovalHistoryEntity.Id)}`) as ApprovalCount," +
                                        $" MIN(`{nameof(ApprovalHistoryEntity.TransitionTime)}`) as FirstApprovalTime," +
                                        $" MAX(`{nameof(ApprovalHistoryEntity.TransitionTime)}`) as LastApprovalTime" +
                                        $" FROM {DbTableName}" +
                                        $" WHERE `{nameof(ApprovalHistoryEntity.IdentityId)}` =  @{paramName}" +
                                        $" Group by `{nameof(ApprovalHistoryEntity.ProcessId)}`) a" +

                                $" Order By LastApprovalTime Desc ";

            if (paging != null)
            {
                selectText += $" LIMIT {paging.PageSize} OFFSET {paging.SkipCount()}";
            }

            var param = new MySqlParameter(paramName, MySqlDbType.VarString) {Value = identityId};
            return (await SelectAsDictionaryAsync(connection, selectText, param).ConfigureAwait(false))
                .Select(OutboxItemFromDictionary).ToList();
        }

        public async Task<int> GetOutboxCountByIdentityIdAsync(MySqlConnection connection, string identityId)
        {
            string paramName = "_" + nameof(identityId);
            string selectText = $"SELECT  COUNT(a.{nameof(ApprovalHistoryEntity.ProcessId)})" +
                                $" FROM " +
                                $" (SELECT `{nameof(ApprovalHistoryEntity.ProcessId)}`" +
                                $" FROM {DbTableName}" +
                                $" WHERE `{nameof(ApprovalHistoryEntity.IdentityId)}` =  @{paramName}" +
                                $" Group by `{nameof(ApprovalHistoryEntity.ProcessId)}`) a";

            var param = new MySqlParameter(paramName, MySqlDbType.VarString) {Value = identityId};
            var result = await ExecuteCommandScalarAsync(connection, selectText, param).ConfigureAwait(false);

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
                    {
                        if (field.Value is byte[] bytes)
                        {
                            item.ProcessId = new Guid(bytes);
                        }
                        else
                        {
                            item.ProcessId = (Guid)field.Value;
                        }

                        break;
                    }
                    case nameof(item.ApprovalCount):
                        item.ApprovalCount = Convert.ToInt32(field.Value);
                        break;
                    case nameof(item.FirstApprovalTime):
                        item.FirstApprovalTime = (DateTime?)field.Value;
                        break;
                    case nameof(item.LastApprovalTime):
                        item.LastApprovalTime = (DateTime?)field.Value;
                        break;
                    case nameof(item.LastApproval):
                        item.LastApproval = field.Value as string;
                        break;
                }
            }

            return item;
        }

        public async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId,
            MySqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<List<ApprovalHistoryEntity>> SelectByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            return (await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false)).ToList();
        }

        public async Task<List<ApprovalHistoryEntity>> SelectByIdentityIdAsync(MySqlConnection connection, string identityId)
        {
            return (await SelectByAsync(connection, x => x.IdentityId, identityId)
                .ConfigureAwait(false)).ToList();
        }
    }
}
