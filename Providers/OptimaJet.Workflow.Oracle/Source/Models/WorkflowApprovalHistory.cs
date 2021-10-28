using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
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
                new ColumnInfo {Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name="ProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name="IdentityId"},
                new ColumnInfo {Name="AllowedTo"},
                new ColumnInfo {Name="TransitionTime", Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name="Sort", Type = OracleDbType.Int64},
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
                "Id" => Id.ToByteArray(),
                "ProcessId" => ProcessId.ToByteArray(),
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
                    Id = new Guid((byte[])value);
                    break;
                case "ProcessId":
                    ProcessId = new Guid((byte[])value);
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
                    Sort = (long)(decimal)value;
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
        
        public static WorkflowApprovalHistory ToDb(ApprovalHistoryItem historyItem)
        {
            return new WorkflowApprovalHistory()
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
        public static ApprovalHistoryItem FromDb(WorkflowApprovalHistory historyItem)
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

        public static async Task<List<OutboxItem>> SelectOutboxByIdentityIdAsync(OracleConnection connection, string identityId, Paging paging = null)
        {
            string paramName = $"p_{nameof(identityId)}";
            string selectText = $"SELECT  a.ProcessId, a.ApprovalCount, a.FirstApprovalTime, a.LastApprovalTime," +

                                $" (SELECT {"TriggerName".ToUpperInvariant()} FROM {ObjectName} c" +
                                $" WHERE c.ProcessId = a.ProcessId " +
                                $" and c.IdentityId =  :{paramName}" +
                                $" ORDER BY c.TransitionTime DESC FETCH NEXT 1 ROWS ONLY) as LastApproval" +

                                $" FROM " +
                                $" (SELECT {"ProcessId".ToUpperInvariant()}," +
                                $" Count({"Id".ToUpperInvariant()}) as ApprovalCount," +
                                $" MIN({"TransitionTime".ToUpperInvariant()}) as FirstApprovalTime," +
                                $" MAX({"TransitionTime".ToUpperInvariant()}) as LastApprovalTime" +
                                $" FROM {ObjectName}" +
                                $" WHERE {"IdentityId".ToUpperInvariant()} =  :{paramName}" +
                                $" Group by {"ProcessId".ToUpperInvariant()}) a" +

                                $" Order By LastApprovalTime Desc ";
            
            if (paging != null)
            {
                selectText += $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }
            
            var param = new OracleParameter(paramName, OracleDbType.NVarchar2, ParameterDirection.Input) { Value = ConvertToDbCompatibilityType(identityId) };
           return (await SelectAsDictionaryAsync(connection, selectText, param).ConfigureAwait(false))
                .Select(OutboxItemFromDictionary).ToList();
        }

        public static async Task<int> GetOutboxCountByIdentityIdAsync(OracleConnection connection, string identityId)
        {
            string paramName = $"p_{nameof(identityId)}";
            string selectText = $"SELECT  COUNT(a.ProcessId)" +
                                $" FROM " +
                                $" (SELECT {"ProcessId".ToUpperInvariant()} " +
                                $" FROM {ObjectName}" +
                                $" WHERE {"IdentityId".ToUpperInvariant()} =  :{paramName}" +
                                $" Group by {"ProcessId".ToUpperInvariant()}) a";
            
            var param = new OracleParameter(paramName, OracleDbType.NVarchar2, ParameterDirection.Input) { Value = ConvertToDbCompatibilityType(identityId) };
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
                    case string name when name == nameof(item.ProcessId).ToUpper():
                    {
                        if(field.Value is byte[] bytes)
                        {
                            item.ProcessId = new Guid(bytes);
                        }
                        else
                        {
                            item.ProcessId = (Guid)field.Value;
                        }
                        break;

                    }
                    case string name when name == nameof(item.ApprovalCount).ToUpper(): item.ApprovalCount = Convert.ToInt32(field.Value); break;
                    case string name when name == nameof(item.FirstApprovalTime).ToUpper(): item.FirstApprovalTime = (DateTime?)field.Value; break;
                    case string name when name == nameof(item.LastApprovalTime).ToUpper(): item.LastApprovalTime = (DateTime?)field.Value; break;
                    case string name when name == nameof(item.LastApproval).ToUpper(): item.LastApproval = field.Value as string; break;
                }
            }
            
            return item;
        }

        public static async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }
        
        public static async Task<List<WorkflowApprovalHistory>> SelectByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            return (await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false)).ToList();
        }
        
        public static async Task<List<WorkflowApprovalHistory>> SelectByIdentityIdAsync(OracleConnection connection, string identityId)
        {
            return (await SelectByAsync(connection, x => x.IdentityId, identityId)
                .ConfigureAwait(false)).ToList();
        }
    }
}
