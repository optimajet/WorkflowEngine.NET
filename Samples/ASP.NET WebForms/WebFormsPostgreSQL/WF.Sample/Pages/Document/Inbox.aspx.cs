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
    public partial class Inbox : DocumentsPage<InboxDocumentModel>
    {
        public override List<InboxDocumentModel> GetDocuments(out int count, int pageNumber, int pageSize)
        {
            string identityId = CurrentUserSettings.GetCurrentUser().ToString();
            
            List<InboxItem> inbox = WorkflowInit.Runtime.PersistenceProvider
                .GetInboxByIdentityIdAsync(identityId, Paging.Create(pageNumber, pageSize)).Result;
            
            count = WorkflowInit.Runtime.PersistenceProvider.GetInboxCountByIdentityIdAsync(identityId).Result;
            return GetDocumentsByInbox(inbox);

        }
        
        private List<InboxDocumentModel> GetDocumentsByInbox(List<InboxItem> inbox)
        {
            var ids = inbox.Select(x => x.ProcessId).ToList();
            
            var documents = DocumentRepository.GetByIds(ids)
                .ToDictionary(x=>x.Id, x=>x);
            
            var docs = new List<InboxDocumentModel>();
            
            foreach (InboxItem inboxItem in inbox)
            {
                InboxDocumentModel doc;
                
                //if document is exists
                if (documents.TryGetValue(inboxItem.ProcessId, out Business.Model.Document _doc))
                {
                    doc = _doc.ToDocumentModel<InboxDocumentModel>();
                }
                else
                {
                    doc = new InboxDocumentModel();
                    doc.Id = inboxItem.ProcessId;
                    doc.IsCorrect = false;
                    doc.StateName = DocumentModel.NotFoundError;
                }

                doc.AvailableCommands = inboxItem.AvailableCommands;
                doc.AddingDate = inboxItem.AddingDate.ToString();
                docs.Add(doc);
            }

            return docs;
        }
     
        
    }
}
