using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using WorkflowRuntime = OptimaJet.Workflow.Core.Runtime.WorkflowRuntime;

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowInbox : DbObject<WorkflowInbox>
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }
        public DateTime AddingDate { get; set; }
        public string AvailableCommands { get; set; }

        static WorkflowInbox()
        {
            DbTableName = "WorkflowInbox";
        }

        public WorkflowInbox()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name="ProcessId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name="IdentityId", Type = SqlDbType.NVarChar},
                new ColumnInfo {Name="AddingDate", Type = SqlDbType.DateTime},
                new ColumnInfo {Name="AvailableCommands", Type = SqlDbType.NVarChar}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "ProcessId":
                    return ProcessId;
                case "IdentityId":
                    return IdentityId;
                case "AddingDate":
                    return AddingDate;
                case "AvailableCommands":
                    return AvailableCommands;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id =  (Guid)value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid)value;
                    break;
                case "IdentityId":
                    IdentityId = value as string;
                    break;
                case "AddingDate":
                    AddingDate = (DateTime)value;
                    break;
                case "AvailableCommands":
                    AvailableCommands = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public static WorkflowInbox ToDB(InboxItem inboxItem)
        {
            return new WorkflowInbox()
            {
                Id = inboxItem.Id,
                ProcessId = inboxItem.ProcessId,
                IdentityId = inboxItem.IdentityId,
                AddingDate = inboxItem.AddingDate,
                AvailableCommands =  HelperParser.Join(",", inboxItem.AvailableCommands?.Select(x=>x.Name))
            };
        }
        public static async Task<List<InboxItem>> FromDB(WorkflowRuntime runtime, WorkflowInbox [] inboxItems, CultureInfo culture)
        { 
            var result = new List<InboxItem>();
            IEnumerable<IGrouping<Guid, WorkflowInbox>> groups =  inboxItems.GroupBy(x => x.ProcessId);
            ProcessInstance processInstance;
            foreach (IGrouping<Guid, WorkflowInbox> group in groups)
            {
                try
                {
                    processInstance = await runtime.Builder.GetProcessInstanceAsync(group.Key).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    processInstance = null;
                }

                foreach (WorkflowInbox inboxItem in group)
                {
                    List<string> availableCommands = HelperParser.SplitWithTrim(inboxItem.AvailableCommands, ",");
                    result.Add(new InboxItem()
                    {
                        Id = inboxItem.Id,
                        ProcessId = inboxItem.ProcessId,
                        IdentityId = inboxItem.IdentityId,
                        AddingDate = inboxItem.AddingDate,
                        AvailableCommands = availableCommands.Select(x=>
                            new CommandName()
                            {
                                Name = x, 
                                LocalizedName = processInstance?.GetLocalizedCommandName(x, culture)
                            }).ToList()
                    });
                }
            }

            return result;
        }
        
        public static async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId,
            SqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }
        
        public static async Task<WorkflowInbox[]> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            return await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }

    }
}
