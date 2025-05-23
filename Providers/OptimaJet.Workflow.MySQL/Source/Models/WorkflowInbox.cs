using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowInbox : DbObject<InboxEntity>
    {
        public WorkflowInbox(int commandTimeout) : base("workflowinbox", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(InboxEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(InboxEntity.ProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(InboxEntity.IdentityId), Type = MySqlDbType.String},
                new ColumnInfo {Name = nameof(InboxEntity.AddingDate), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(InboxEntity.AvailableCommands), Type = MySqlDbType.String}
            });
        }

        public static InboxEntity ToDB(InboxItem inboxItem)
        {
            return new InboxEntity()
            {
                Id = inboxItem.Id,
                ProcessId = inboxItem.ProcessId,
                IdentityId = inboxItem.IdentityId,
                AddingDate = inboxItem.AddingDate,
                AvailableCommands = HelperParser.Join(",", inboxItem.AvailableCommands?.Select(x => x.Name))
            };
        }

        public static async Task<List<InboxItem>> FromDB(WorkflowRuntime runtime, InboxEntity[] inboxItems, CultureInfo culture)
        {
            var result = new List<InboxItem>();
            IEnumerable<IGrouping<Guid, InboxEntity>> groups = inboxItems.GroupBy(x => x.ProcessId);
            ProcessInstance processInstance;
            foreach (IGrouping<Guid, InboxEntity> group in groups)
            {
                try
                {
                    processInstance = await runtime.Builder.GetProcessInstanceAsync(group.Key).ConfigureAwait(false);
                }
                catch (Exception ex)
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
                        AvailableCommands = availableCommands.Select(x =>
                                new CommandName()
                                {
                                    Name = x,
                                    LocalizedName = processInstance?.GetLocalizedCommandName(x, culture)
                                })
                            .ToList()
                    });
                }
            }

            return result;
        }

        public async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId,
            MySqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<InboxEntity[]> SelectByProcessIdAsync(MySqlConnection connection, Guid processId)
        {
            return await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }
    }
}
