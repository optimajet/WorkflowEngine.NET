using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Entities;
using WorkflowRuntime = OptimaJet.Workflow.Core.Runtime.WorkflowRuntime;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowInbox : DbObject<InboxEntity>
    {
        public WorkflowInbox(string schemaName, int commandTimeout) : base(schemaName, "WorkflowInbox", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(InboxEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(InboxEntity.ProcessId), Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(InboxEntity.IdentityId), Type = SqlDbType.NVarChar},
                new ColumnInfo {Name = nameof(InboxEntity.AddingDate), Type = SqlDbType.DateTime},
                new ColumnInfo {Name = nameof(InboxEntity.AvailableCommands), Type = SqlDbType.NVarChar}
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

            foreach (var group in groups)
            {
                try
                {
                    processInstance = await runtime.Builder.GetProcessInstanceAsync(group.Key).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    processInstance = null;
                }

                foreach (InboxEntity inboxItem in group)
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
                                Name = x, LocalizedName = processInstance?.GetLocalizedCommandName(x, culture)
                            }).ToList()
                    });
                }
            }

            return result;
        }

        public async Task<int> DeleteByProcessIdAsync(SqlConnection connection, Guid processId, SqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public async Task<InboxEntity[]> SelectByProcessIdAsync(SqlConnection connection, Guid processId)
        {
            return await SelectByAsync(connection, x => x.ProcessId, processId)
                .ConfigureAwait(false);
        }
    }
}
