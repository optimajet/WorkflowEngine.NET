using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;
using StackExchange.Redis;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.Redis.Implementation
{
    public class InboxRepository : RepositoryBase, IInboxRepository
    {
        public InboxRepository(ConnectionSettingsProvider settings) : base(settings)
        {
        }

        public void DropWorkflowInbox(Guid processId)
        {
            var db = _connector.GetDatabase();

            var batch = db.CreateBatch();

            var employeesKey = GetKeyForInboxEmployees(processId);
            var inboxEmployees = db.SetMembers(employeesKey);

            foreach (var employeeIndex in inboxEmployees)
            {
                if (employeeIndex.HasValue)
                {
                    var guid = new Guid(employeeIndex.ToString());
                    batch.ListRemoveAsync(GetKeyForInboxDocuments(guid), processId.ToString());
                }
            }

            batch.KeyDeleteAsync(employeesKey);

            batch.Execute();
        }

        public void FillInbox(Guid processId, WorkflowRuntime workflowRuntime)
        {
            var newActors = workflowRuntime.GetAllActorsForDirectCommandTransitions(processId);

            if (newActors.Count() > 0)
            {
                var db = _connector.GetDatabase();

                var batch = db.CreateBatch();

                batch.SetAddAsync(GetKeyForInboxEmployees(processId), newActors.Select(x => (RedisValue)x).ToArray());

                foreach (var newActor in newActors)
                {
                    batch.ListRightPushAsync(GetKeyForInboxDocuments(newActor), processId.ToString());
                }

                batch.Execute();
            }
        }

        public void RecalcInbox(WorkflowRuntime workflowRuntime)
        {
            var db = _connector.GetDatabase();

            var docs = db.SortedSetRangeByRank(GetKeyForSortedDocuments());

            foreach(var doc in docs)
            {
                Guid id = new Guid(doc.ToString());
                try
                {
                    if (workflowRuntime.IsProcessExists(id))
                    {
                        workflowRuntime.UpdateSchemeIfObsolete(id);
                        DropWorkflowInbox(id);
                        FillInbox(id, workflowRuntime);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to calculate the inbox for process Id = {0}", id), ex);
                }
            }
        }
    }
}
