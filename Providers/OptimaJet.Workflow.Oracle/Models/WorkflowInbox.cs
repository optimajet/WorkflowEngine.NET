using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle
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
                new ColumnInfo {Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name="ProcessId", Type = OracleDbType.Raw},
                new ColumnInfo {Name="IdentityId", Type = OracleDbType.NChar},
                new ColumnInfo {Name="AddingDate", Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name="AvailableCommands", Type = OracleDbType.NChar}
            });
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
        
        public override object GetValue(string key)
        {
            return key switch
            {
                "Id" => Id.ToByteArray(),
                "ProcessId" => ProcessId.ToByteArray(),
                "IdentityId" => IdentityId as string,
                "AddingDate" => AddingDate,
                "AvailableCommands" => AvailableCommands,
                _ => throw new Exception(string.Format("Column {0} is not exists", key))
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
                case "AddingDate":
                    AddingDate = (DateTime)value;
                    break;
                case "AvailableCommands":
                    AvailableCommands = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }
        public static async Task<WorkflowInbox[]> SelectByProcessIdAsync(OracleConnection connection, Guid processId)
        {
            return await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }

    }
}
