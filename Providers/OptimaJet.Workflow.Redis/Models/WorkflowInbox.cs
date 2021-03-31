using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Redis
{
    public class WorkflowInbox
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }
        public DateTime AddingDate { get; set; }
        public string AvailableCommands { get; set; }
        
        public static WorkflowInbox ToDB(InboxItem inboxItem)
        {
            return new WorkflowInbox()
            {
                Id = inboxItem.Id,
                ProcessId = inboxItem.ProcessId,
                IdentityId = inboxItem.IdentityId,
                AddingDate = inboxItem.AddingDate,
                AvailableCommands = HelperParser.Join(",", inboxItem.AvailableCommands?.Select(x=>x.Name))
            };
        }
        public static async Task<List<InboxItem>> FromDB(WorkflowRuntime runtime, WorkflowInbox [] inboxItems, CultureInfo culture)
        { 
            var result = new List<InboxItem>();
            IEnumerable<IGrouping<Guid, WorkflowInbox>> groups =  inboxItems.GroupBy(x => x.ProcessId);
            ProcessInstance processInstance;
            foreach (var group in groups)
            {
                try
                {
                    processInstance = await runtime.Builder.GetProcessInstanceAsync(group.Key).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    processInstance = null;
                }

                foreach (var inboxItem in group)
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

    }
}
