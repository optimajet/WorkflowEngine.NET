using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowApprovalHistory : DbObject<ApprovalHistoryEntity>
    {
        public WorkflowApprovalHistory(string schemaName, int commandTimeout) : base(schemaName, "WorkflowApprovalHistory", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.ProcessId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.IdentityId)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.AllowedTo)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.TransitionTime), Type = SqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Sort), Type = SqlDbType.BigInt},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.InitialState)},
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

        public async Task<List<OutboxItem>> SelectOutboxByIdentityIdAsync(SqlConnection connection, string identityId, Paging paging = null)
        {
            string paramName = "_" + nameof(identityId);
            string selectText = $"SELECT  a.{nameof(ApprovalHistoryEntity.ProcessId)}, a.ApprovalCount, a.FirstApprovalTime, a.LastApprovalTime," +

                                        $" (SELECT TOP 1 [{nameof(ApprovalHistoryEntity.TriggerName)}] FROM {ObjectName} c" +
                                        $" WHERE c.ProcessId = a.ProcessId " +
                                        $" and c.IdentityId =  @{paramName}" +
                                        $" ORDER BY c.TransitionTime DESC) as LastApproval" +

                                $" FROM " +
                                        $" (SELECT [{nameof(ApprovalHistoryEntity.ProcessId)}]," +
                                        $" Count({nameof(ApprovalHistoryEntity.Id)}) as ApprovalCount," +
                                        $" MIN({nameof(ApprovalHistoryEntity.TransitionTime)}) as FirstApprovalTime," +
                                        $" MAX({nameof(ApprovalHistoryEntity.TransitionTime)}) as LastApprovalTime" +
                                        $" FROM {ObjectName}" +
                                        $" WHERE {nameof(ApprovalHistoryEntity.IdentityId)} =  @{paramName}" +
                                        $" Group by [{nameof(ApprovalHistoryEntity.ProcessId)}]) a" +

                                $" Order By LastApprovalTime Desc ";

            if (paging != null)
            {
                selectText += $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }

            var param = new SqlParameter(paramName, SqlDbType.NVarChar) {Value = identityId};
            return (await SelectAsDictionaryAsync(connection, selectText, param).ConfigureAwait(false))
                .Select(OutboxItemFromDictionary).ToList();
        }

        private static OutboxItem OutboxItemFromDictionary(Dictionary<string, object> fields)
        {
            var item = new OutboxItem();
            foreach (KeyValuePair<string, object> field in fields)
            {
                switch (field.Key)
                {
                    case nameof(item.ProcessId):
                        item.ProcessId = (Guid)field.Value;
                        break;
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

        public async Task<int> GetOutboxCountByIdentityIdAsync(SqlConnection connection, string identityId)
        {
            string paramName = "_" + nameof(identityId);
            string selectText = $"SELECT  COUNT(a.{nameof(ApprovalHistoryEntity.ProcessId)})" +
                                $" FROM " +
                                $" (SELECT [{nameof(ApprovalHistoryEntity.ProcessId)}] " +
                                $" FROM {ObjectName}" +
                                $" WHERE {nameof(ApprovalHistoryEntity.IdentityId)} =  @{paramName}" +
                                $" Group by [{nameof(ApprovalHistoryEntity.ProcessId)}]) a";

            var param = new SqlParameter(paramName, SqlDbType.NVarChar) {Value = identityId};
            object result = await ExecuteCommandScalarAsync(connection, selectText, param).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId,
            SqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<List<ApprovalHistoryEntity>> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            return (await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false)).ToList();
        }

        public async Task<List<ApprovalHistoryEntity>> SelectByIdentityIdAsync(SqlConnection connection,
            string identityId)
        {
            return (await SelectByAsync(connection, x => x.IdentityId, identityId)
                .ConfigureAwait(false)).ToList();
        }
    }
}
