using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using WF.Sample.Business.DataAccess;
using WF.Sample.Helpers;
using WF.Sample.Models;

namespace WF.Sample.Pages.Document
{
    public partial class Index : DocumentsPage<DocumentModel>
    {
        public override List<DocumentModel> GetDocuments(out int count, int pageNumber, int pageSize)
        {
            return DocumentRepository.Get(out count, pageNumber, pageSize).Select(x=>x.ToDocumentModel<DocumentModel>()).ToList();
        }
    }
}
