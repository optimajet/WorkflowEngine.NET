using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.Document
{
    public class DeleteRowsHandler : IHttpHandler
    {

        public IDocumentRepository DocumentRepository { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            var ids = context.Request.Params["ids"]?.Split(',');

            if (ids == null || ids.Length == 0)
            {
                context.Response.Write("Items not selected");
                return;
            }

            try
            {
                var guids = ids.Select(x => new Guid(x)).ToArray();
                DocumentRepository.Delete(guids);
            }
            catch (Exception ex)
            {
                context.Response.Write(ex.Message);
                return;
            }

            context.Response.Write("Rows deleted");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}