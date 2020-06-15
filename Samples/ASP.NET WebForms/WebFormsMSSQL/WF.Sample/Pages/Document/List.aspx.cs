using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WF.Sample.Business.DataAccess;
using WF.Sample.Helpers;
using WF.Sample.Models;

namespace WF.Sample.Pages.Document
{
    public partial class List : Page
    {
        protected const int PageSize = 15;

        protected int PageNumber { get; set; }
        protected int Count { get; set; }

        public IDocumentRepository DocumentRepository { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            PageNumber = Convert.ToInt32(Request.QueryString["page"]);
            var docs = GetDocuments(out int count);
            Count = count;

            DocsTableRepeater.DataSource = docs;
            DocsTableRepeater.DataBind();
        }

        private List<DocumentModel> GetDocuments(out int count)
        {
            switch (RouteData.Values["action"]?.ToString())
            {
                case "Inbox":
                    return DocumentRepository.GetInbox(CurrentUserSettings.GetCurrentUser(), out count, PageNumber, PageSize)
                        .Select(c => GetDocumentModel(c)).ToList();
                case "Outbox":
                    return DocumentRepository.GetOutbox(CurrentUserSettings.GetCurrentUser(), out count, PageNumber, PageSize)
                        .Select(c => GetDocumentModel(c)).ToList();
                default:
                    return DocumentRepository.Get(out count, PageNumber, PageSize).Select(c => GetDocumentModel(c)).ToList();
            }
        }

        protected string GetActionName()
        {
            var action = RouteData.Values["action"]?.ToString() ?? "Index";
            return action == "Index" ? string.Empty : "/Document/" + action;
        }

        private DocumentModel GetDocumentModel(Business.Model.Document d)
        {
            return new DocumentModel()
            {
                Id = d.Id,
                AuthorId = d.AuthorId,
                AuthorName = d.Author.Name,
                Comment = d.Comment,
                ManagerId = d.ManagerId,
                ManagerName = d.ManagerId.HasValue ? d.Manager.Name : string.Empty,
                Name = d.Name,
                Number = d.Number,
                StateName = d.StateName,
                Sum = d.Sum
            };
        }
    }
}