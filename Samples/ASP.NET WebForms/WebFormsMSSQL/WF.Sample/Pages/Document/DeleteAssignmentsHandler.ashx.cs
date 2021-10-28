using System;
using System.Linq;
using System.Web;
using WF.Sample.Business.Workflow;

namespace WF.Sample.Document
{
    public class DeleteAssignmentsHandler : IHttpHandler
    {
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
                
                foreach (var id in guids)
                {
                    bool result = WorkflowInit.Runtime.AssignmentApi.DeleteAssignmentAsync(id).Result;
                }
            }
            catch (Exception ex)
            {
                context.Response.Write(ex.Message);
                return;
            }
            context.Response.Redirect($"~/Document/Assignments");
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
