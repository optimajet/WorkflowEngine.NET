using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.MsSql
{
    public class InboxRepository : IInboxRepository
    {
        public void DropWorkflowInbox(Guid processId)
        {
            using (var context = new Business.DataModelDataContext())
            {
                context.DropWorkflowInbox(processId);
                context.SubmitChanges();
            }
        }

        public void FillInbox(Guid processId, WorkflowRuntime workflowRuntime)
        {
            using (var context = new Business.DataModelDataContext())
            {
                FillInbox(processId, workflowRuntime, context);

                context.SubmitChanges();
            }
        }

        public void RecalcInbox(WorkflowRuntime workflowRuntime)
        {
            using (var context = new Business.DataModelDataContext())
            {
                foreach (var d in context.Documents.ToList())
                {
                    Guid id = d.Id;
                    try
                    {
                        if (workflowRuntime.IsProcessExists(id))
                        {
                            workflowRuntime.UpdateSchemeIfObsolete(id);
                            context.DropWorkflowInbox(id);
                            FillInbox(id, workflowRuntime, context);

                            context.SubmitChanges();

                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Unable to calculate the inbox for process Id = {0}", id), ex);
                    }
                }
            }
        }

        private void FillInbox(Guid processId, WorkflowRuntime workflowRuntime, Business.DataModelDataContext context)
        {
            var newActors = workflowRuntime.GetAllActorsForDirectCommandTransitions(processId);
            foreach (var newActor in newActors)
            {
                var newInboxItem = new Business.WorkflowInbox() { Id = Guid.NewGuid(), IdentityId = new Guid(newActor), ProcessId = processId };
                context.WorkflowInboxes.InsertOnSubmit(newInboxItem);
            }
        }
    }
}
