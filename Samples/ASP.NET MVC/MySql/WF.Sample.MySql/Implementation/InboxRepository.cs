using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.MySql.Implementation
{
    public class InboxRepository : IInboxRepository
    {
        private readonly SampleContext _sampleContext;

        public InboxRepository(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        public void DropWorkflowInbox(Guid processId)
        {
            DropWorkflowInboxWithNoSave(processId);
            _sampleContext.SaveChanges();
        }

        private void DropWorkflowInboxWithNoSave(Guid processId)
        {
            var bytes = processId.ToByteArray();
            var toDelete = _sampleContext.WorkflowInboxes.Where(x => x.ProcessId == bytes);
            _sampleContext.WorkflowInboxes.RemoveRange(toDelete);
        }

        public void RecalcInbox(WorkflowRuntime workflowRuntime)
        {
            foreach (var d in _sampleContext.Documents.ToList())
            {
                Guid id = new Guid(d.Id);
                try
                {
                    if (workflowRuntime.IsProcessExists(id))
                    {
                        workflowRuntime.UpdateSchemeIfObsolete(id);
                        DropWorkflowInboxWithNoSave(id);
                        FillInbox(id, workflowRuntime);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to calculate the inbox for process Id = {0}", id), ex);
                }

            }
        }

        public void FillInbox(Guid processId, WorkflowRuntime workflowRuntime)
        {
            var newActors = workflowRuntime.GetAllActorsForDirectCommandTransitions(processId);
            var processIdBytes = processId.ToByteArray();
            foreach (var newActor in newActors)
            {
                var newInboxItem = new WorkflowInbox() { Id = Guid.NewGuid().ToByteArray(), IdentityId = newActor, ProcessId = processIdBytes };
                _sampleContext.WorkflowInboxes.Add(newInboxItem);
            }
            _sampleContext.SaveChanges();
        }
    }
}
