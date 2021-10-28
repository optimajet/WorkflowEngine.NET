using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowApprovalHistory : DbObject<WorkflowApprovalHistory>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }
        public string AllowedTo { get; set; }
        public DateTime? TransitionTime { get; set; }
        public long Sort { get; set; }
        public string InitialState { get; set; }
        public string DestinationState { get; set; }
        public string TriggerName { get; set; }
        public string Commentary { get; set; }
        

        static WorkflowApprovalHistory()
        {
            DbTableName = "WorkflowApprovalHistory";
        }
       
       public WorkflowApprovalHistory()
       {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name="ProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name="IdentityId"},
                new ColumnInfo {Name="AllowedTo"},
                new ColumnInfo {Name="TransitionTime", Type = SqlDbType.DateTime},
                new ColumnInfo {Name="Sort", Type = SqlDbType.BigInt},
                new ColumnInfo {Name="InitialState"},
                new ColumnInfo {Name="DestinationState"},
                new ColumnInfo {Name="TriggerName"},
                new ColumnInfo {Name="Commentary"},
            });
        }
       
       public override object GetValue(string key)
       {
           return key switch
           {
               "Id" => Id,
               "ProcessId" => ProcessId,
               "IdentityId" => IdentityId,
               "AllowedTo" => AllowedTo,
               "TransitionTime" => TransitionTime,
               "Sort" => Sort,
               "InitialState" => InitialState,
               "DestinationState" => DestinationState,
               "TriggerName" => TriggerName,
               "Commentary" => Commentary,
               _ => throw new Exception($"Column {key} is not exists")
           };
       }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = (Guid)value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid)value;
                    break;
                case "IdentityId":
                    IdentityId = value as string;
                    break;
                case "AllowedTo":
                    AllowedTo = value as string;
                    break;
                case "TransitionTime":
                    {
                        TransitionTime = null;
                        if (value != null)
                        {
                            TransitionTime = (DateTime?)value;
                        }

                    }
                    break;
                case "Sort":
                    Sort = (long)value;
                    break;
                case "InitialState":
                    InitialState = value as string;
                    break;
                case "DestinationState":
                    DestinationState = value as string;
                    break;
                case "TriggerName":
                    TriggerName = value as string;
                    break;
                case "Commentary":
                    Commentary = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }
        
        public static WorkflowApprovalHistory ToDB(ApprovalHistoryItem historyItem)
        {
            return new WorkflowApprovalHistory()
            {
                Id = historyItem.Id,
                ProcessId = historyItem.ProcessId,
                IdentityId = historyItem.IdentityId,
                AllowedTo =  HelperParser.Join(",", historyItem.AllowedTo),
                TransitionTime = historyItem.TransitionTime,
                Sort = historyItem.Sort,
                InitialState = historyItem.InitialState,
                DestinationState = historyItem.DestinationState,
                TriggerName = historyItem.TriggerName,
                Commentary = historyItem.Commentary,
            };
        }
        public static ApprovalHistoryItem FromDB(WorkflowApprovalHistory historyItem)
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

        public static async Task<List<OutboxItem>> SelectOutboxByIdentityIdAsync(SqlConnection connection, string identityId, Paging paging = null)
        {
            string paramName = "_"+nameof(identityId);
            string selectText = $"SELECT  a.ProcessId, a.ApprovalCount, a.FirstApprovalTime, a.LastApprovalTime," +

                                        $" (SELECT TOP 1 [TriggerName] FROM {ObjectName} c" +
                                        $" WHERE c.ProcessId = a.ProcessId " +
                                        $" and c.IdentityId =  @{paramName}" +
                                        $" ORDER BY c.TransitionTime DESC) as LastApproval" +

                                $" FROM " +
                                        $" (SELECT [ProcessId]," +
                                        $" Count(Id) as ApprovalCount," +
                                        $" MIN(TransitionTime) as FirstApprovalTime," +
                                        $" MAX(TransitionTime) as LastApprovalTime" +
                                        $" FROM {ObjectName}" +
                                        $" WHERE IdentityId =  @{paramName}" +
                                        $" Group by [ProcessId]) a" +

                                $" Order By LastApprovalTime Desc ";
            
            if (paging != null)
            {
                selectText += $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }
            
            var param = new SqlParameter(paramName, SqlDbType.NVarChar) { Value = identityId };
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
                    case nameof(item.ProcessId): item.ProcessId = (Guid)field.Value; break;
                    case nameof(item.ApprovalCount): item.ApprovalCount = Convert.ToInt32(field.Value); break;
                    case nameof(item.FirstApprovalTime): item.FirstApprovalTime = (DateTime?)field.Value; break;
                    case nameof(item.LastApprovalTime): item.LastApprovalTime = (DateTime?)field.Value; break;
                    case nameof(item.LastApproval): item.LastApproval = field.Value as string; break;
                }
            }

            return item;
        }
        public static async Task<int> GetOutboxCountByIdentityIdAsync(SqlConnection connection, string identityId)
        {
            string paramName = "_"+nameof(identityId);
            string selectText = $"SELECT  COUNT(a.ProcessId)" +
                                $" FROM " +
                                $" (SELECT [ProcessId] " +
                                $" FROM {ObjectName}" +
                                $" WHERE IdentityId =  @{paramName}" +
                                $" Group by [ProcessId]) a";
            
            var param = new SqlParameter(paramName, SqlDbType.NVarChar) { Value = identityId };
            object result = await ExecuteCommandScalarAsync(connection, selectText, param).ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        
        public static async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }
        
        public static async Task<List<WorkflowApprovalHistory>> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            return (await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false)).ToList();
        }
        
        public static async Task<List<WorkflowApprovalHistory>> SelectByIdentityIdAsync(SqlConnection connection, string identityId)
        {
            return (await SelectByAsync(connection, x => x.IdentityId, identityId)
                .ConfigureAwait(false)).ToList();
        }
    }
}
