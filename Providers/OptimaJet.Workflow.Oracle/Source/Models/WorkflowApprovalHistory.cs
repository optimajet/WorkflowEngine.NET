using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowApprovalHistory : DbObject<ApprovalHistoryEntity>
    {
        public WorkflowApprovalHistory(string schemaName, int commandTimeout) : base(schemaName, "WorkflowApprovalHistory", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Id), IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.ProcessId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.IdentityId)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.AllowedTo)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.TransitionTime), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Sort), Type = OracleDbType.Decimal},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.InitialState)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.DestinationState)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.TriggerName)},
                new ColumnInfo {Name = nameof(ApprovalHistoryEntity.Commentary)},
            });
        }

        public static ApprovalHistoryEntity ToDb(ApprovalHistoryItem historyItem)
        {
            return new ApprovalHistoryEntity
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

        public static ApprovalHistoryItem FromDb(ApprovalHistoryEntity historyItem)
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

        public async Task<List<OutboxItem>> SelectOutboxByIdentityIdAsync(OracleConnection connection, string identityId,
            Paging paging = null)
        {
            string paramName = $"p_{nameof(identityId)}";
            string selectText = $"SELECT  a.{nameof(ApprovalHistoryEntity.ProcessId)}, a.ApprovalCount, a.FirstApprovalTime, a.LastApprovalTime," +
                                
                                $" (SELECT {nameof(ApprovalHistoryEntity.TriggerName).ToUpperInvariant()} FROM {ObjectName} c" +
                                $" WHERE c.{nameof(ApprovalHistoryEntity.ProcessId)} = a.{nameof(ApprovalHistoryEntity.ProcessId)} " +
                                $" and c.{nameof(ApprovalHistoryEntity.IdentityId)} =  :{paramName}" +
                                $" ORDER BY c.{nameof(ApprovalHistoryEntity.TransitionTime)} DESC FETCH NEXT 1 ROWS ONLY) as LastApproval" +
                                
                                $" FROM " +
                                $" (SELECT {nameof(ApprovalHistoryEntity.ProcessId).ToUpperInvariant()}," +
                                $" Count({nameof(ApprovalHistoryEntity.Id).ToUpperInvariant()}) as ApprovalCount," +
                                $" MIN({nameof(ApprovalHistoryEntity.TransitionTime).ToUpperInvariant()}) as FirstApprovalTime," +
                                $" MAX({nameof(ApprovalHistoryEntity.TransitionTime).ToUpperInvariant()}) as LastApprovalTime" +
                                $" FROM {ObjectName}" +
                                $" WHERE {nameof(ApprovalHistoryEntity.IdentityId).ToUpperInvariant()} =  :{paramName}" +
                                $" Group by {nameof(ApprovalHistoryEntity.ProcessId).ToUpperInvariant()}) a" +
                                
                                $" Order By LastApprovalTime Desc ";

            if (paging != null)
            {
                selectText += $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }

            var param = new OracleParameter(paramName, OracleDbType.NVarchar2, ParameterDirection.Input) {Value = identityId};
            return (await SelectAsDictionaryAsync(connection, selectText, param).ConfigureAwait(false))
                .Select(OutboxItemFromDictionary).ToList();
        }

        public async Task<int> GetOutboxCountByIdentityIdAsync(OracleConnection connection, string identityId)
        {
            string paramName = $"p_{nameof(identityId)}";
            string selectText = $"SELECT  COUNT(a.{nameof(ApprovalHistoryEntity.ProcessId)})" +
                                $" FROM " +
                                $" (SELECT {nameof(ApprovalHistoryEntity.ProcessId).ToUpperInvariant()} " +
                                $" FROM {ObjectName}" +
                                $" WHERE {nameof(ApprovalHistoryEntity.IdentityId).ToUpperInvariant()} =  :{paramName}" +
                                $" Group by {nameof(ApprovalHistoryEntity.ProcessId).ToUpperInvariant()}) a";

            var param = new OracleParameter(paramName, OracleDbType.NVarchar2, ParameterDirection.Input) {Value = identityId};
            var result = await ExecuteCommandScalarAsync(connection, selectText, param).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        private static OutboxItem OutboxItemFromDictionary(Dictionary<string, object> fields)
        {
            var item = new OutboxItem();
            foreach (KeyValuePair<string, object> field in fields)
            {
                string fieldName = field.Key.ToUpper();
                switch (fieldName)
                {
                    case { } name when name == nameof(item.ProcessId).ToUpper():
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
                    case { } name when name == nameof(item.ApprovalCount).ToUpper():
                        item.ApprovalCount = Convert.ToInt32(field.Value);
                        break;
                    case { } name when name == nameof(item.FirstApprovalTime).ToUpper():
                        item.FirstApprovalTime = (DateTime?)field.Value;
                        break;
                    case { } name when name == nameof(item.LastApprovalTime).ToUpper():
                        item.LastApprovalTime = (DateTime?)field.Value;
                        break;
                    case { } name when name == nameof(item.LastApproval).ToUpper():
                        item.LastApproval = field.Value as string;
                        break;
                }
            }

            return item;
        }

        public async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId, OracleTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<List<ApprovalHistoryEntity>> SelectByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            return (await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false)).ToList();
        }

        public async Task<List<ApprovalHistoryEntity>> SelectByIdentityIdAsync(OracleConnection connection, string identityId)
        {
            return (await SelectByAsync(connection, x => x.IdentityId, identityId)
                .ConfigureAwait(false)).ToList();
        }
    }
}
