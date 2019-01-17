using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.MongoDB;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Workflow;

namespace WF.Sample.MongoDb.Implementation
{
    public class InboxRepository : IInboxRepository
    {
        private static IMongoDatabase Store => (WorkflowInit.Runtime.PersistenceProvider as MongoDBProvider).Store;

        public void DropWorkflowInbox(Guid processId)
        {
            var dbcoll = Store.GetCollection<Entities.WorkflowInbox>("WorkflowInbox");
            dbcoll.DeleteMany(c => c.ProcessId == processId);
        }

        public void FillInbox(Guid processId, WorkflowRuntime workflowRuntime)
        {
            var newActors = workflowRuntime.GetAllActorsForDirectCommandTransitions(processId);
            var items = new List<Entities.WorkflowInbox>();
            foreach (var newActor in newActors)
            {
                items.Add(new Entities.WorkflowInbox() { Id = Guid.NewGuid(), IdentityId = newActor, ProcessId = processId });
            }

            if (items.Any())
            {
                var dbcoll = Store.GetCollection<Entities.WorkflowInbox>("WorkflowInbox");
                dbcoll.InsertMany(items);
            }
        }

        public void RecalcInbox(WorkflowRuntime workflowRuntime)
        {
            foreach (var d in Store.GetCollection<Entities.Document>("Document").Find(new BsonDocument()).ToList())
            {
                Guid id = d.Id;
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
