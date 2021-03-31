using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Workflow;
using WF.Sample.Helpers;
using WF.Sample.Models;

namespace WF.Sample.Pages.Document
{
    public partial class Outbox : DocumentsPage<OutboxDocumentModel>
    {
        public override List<OutboxDocumentModel> GetDocuments(out int count, int pageNumber, int pageSize)
        {
            string identityId = CurrentUserSettings.GetCurrentUser().ToString();
            
            List<OutboxItem> outbox =   WorkflowInit.Runtime.PersistenceProvider
                .GetOutboxByIdentityIdAsync(identityId, Paging.Create(pageNumber, pageSize)).Result;
            
            count = WorkflowInit.Runtime.PersistenceProvider.GetOutboxCountByIdentityIdAsync(identityId).Result;

            return GetDocumentsByOutbox(outbox);
        }
        
        private List<OutboxDocumentModel> GetDocumentsByOutbox(List<OutboxItem> outbox)
        {
            var ids = outbox.Select(x => x.ProcessId).ToList();
            
            var documents = DocumentRepository.GetByIds(ids)
                .ToDictionary(x=>x.Id, x=>x);
            
            var docs = new List<OutboxDocumentModel>();
            
            foreach (var outboxItem in outbox)
            {
                OutboxDocumentModel doc;
                
                //if document is exists
                if (documents.TryGetValue(outboxItem.ProcessId, out Business.Model.Document _doc))
                {
                    doc = _doc.ToDocumentModel<OutboxDocumentModel>();
                }
                else
                {
                    doc = new OutboxDocumentModel();
                    doc.Id = outboxItem.ProcessId;
                    doc.IsCorrect = false;
                    doc.StateName = DocumentModel.NotFoundError;
                }

                doc.ApprovalCount = outboxItem.ApprovalCount;
                doc.FirstApprovalTime = outboxItem.FirstApprovalTime;
                doc.LastApprovalTime = outboxItem.LastApprovalTime;
                doc.LastApproval = outboxItem.LastApproval;
                docs.Add(doc);
            }

            return docs;
        }

    }
}
