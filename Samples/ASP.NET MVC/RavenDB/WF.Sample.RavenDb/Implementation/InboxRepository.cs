using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.RavenDB;
using Raven.Client;
using Raven.Client.Document;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Workflow;

namespace WF.Sample.RavenDb.Implementation
{
    public class InboxRepository : IInboxRepository
    {
        private static DocumentStore Store => (WorkflowInit.Runtime.PersistenceProvider as RavenDBProvider).Store;

        public void DropWorkflowInbox(Guid processId)
        {
            using (var session = Store.OpenSession())
            {
                DropWorkflowInbox(processId, session);
                session.SaveChanges();
            }
        }

        public void FillInbox(Guid processId, WorkflowRuntime workflowRuntime)
        {
            using (var session = Store.OpenSession())
            {
                FillInbox(processId, session, workflowRuntime);
                session.SaveChanges();
            }
        }

        public void RecalcInbox(WorkflowRuntime workflowRuntime)
        {
            using (var session = Store.OpenSession())
            {
                foreach (var d in session.Query<Entities.Document>().ToList())
                {
                    Guid id = d.Id;
                    try
                    {
                        if (workflowRuntime.IsProcessExists(id))
                        {
                            workflowRuntime.UpdateSchemeIfObsolete(id);
                            DropWorkflowInbox(id, session);
                            FillInbox(id, session, workflowRuntime);
                            session.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Unable to calculate the inbox for process Id = {0}", id), ex);
                    }

                }
            }
        }

        private void DropWorkflowInbox(Guid processId, IDocumentSession session)
        {
            var inboxDocs = session.Query<Entities.WorkflowInbox>().Where(c => c.ProcessId == processId).ToList();
            foreach (var d in inboxDocs)
                session.Delete(d);
            
        }

        private void FillInbox(Guid processId, IDocumentSession session, WorkflowRuntime workflowRuntime)
        {
            var newActors = workflowRuntime.GetAllActorsForDirectCommandTransitions(processId);
            foreach (var newActor in newActors)
            {
                var newInboxItem = new Entities.WorkflowInbox() { IdentityId = newActor, ProcessId = processId };
                session.Store(newInboxItem);
            }
        }
    }
}
