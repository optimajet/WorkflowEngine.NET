using OptimaJet.Workflow;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using WF.Sample.Business.Workflow;

namespace Designer
{
    /// <summary>
    /// Summary description for WFEDesigner
    /// </summary>
    public class WFEDesigner : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            Stream filestream = null;
            if (context.Request.Files.Count > 0)
                filestream = context.Request.Files[0].InputStream;

            var pars = new NameValueCollection
            {
                context.Request.QueryString
            };

            if (context.Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                var parsKeys = pars.AllKeys;
                foreach (var key in context.Request.Form.AllKeys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(key, context.Request.Unvalidated[key]);
                    }
                }
            }

            var res = WorkflowInit.Runtime.DesignerAPI(pars, out bool hasError, filestream, true);
            
            context.Response.Cache.SetNoStore();
            
            if (pars["operation"].ToLower() == "downloadscheme" && !hasError)
            {
                context.Response.ContentType = "file/xml";
                context.Response.BinaryWrite(Encoding.UTF8.GetBytes(res));
                context.Response.End();
            }
            else
            {
                context.Response.Write(res);
                context.Response.End();
            }
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
