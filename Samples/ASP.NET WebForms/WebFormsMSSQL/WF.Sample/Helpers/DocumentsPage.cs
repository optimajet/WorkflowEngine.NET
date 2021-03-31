using System;
using System.Collections.Generic;
using System.Web.UI;
using WF.Sample.Business.DataAccess;
using WF.Sample.Models;

namespace WF.Sample.Helpers
{
    public abstract partial class DocumentsPage<TDoc>:Page, IPaging
        where TDoc:DocumentModel
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PageNumber = Convert.ToInt32(Request.QueryString["page"] ?? "1");
            var docs = GetDocuments(out int count, PageNumber, PageSize);
            Count = count;
            DocsTableRepeater.DataSource = docs;
            DocsTableRepeater.DataBind();
        }

        public int Count { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 15;
        public abstract List<TDoc> GetDocuments(out int count, int pageNumber, int pageSize);
        
        public IDocumentRepository DocumentRepository { get; set; }
        public IEmployeeRepository EmployeeRepository { get; set; }

        public string GetEditUrl(DocumentModel doc)
        {
            return doc.IsCorrect ? Page.ResolveUrl("~/Document/Edit/" + doc.Id) : "";
        }        

    }
}
