using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using OptimaJet.Workflow;
using WF.Sample.Business.Workflow;


namespace WF.Sample.Controllers
{
    public class DesignerController : Controller
    {
        public ActionResult Index(string schemeName)
        {
            return View();
        }
        
        public ActionResult API()
        {
            Stream filestream = null;
            if (Request.Files.Count > 0)
                filestream = Request.Files[0].InputStream;

            var pars = new NameValueCollection();
            pars.Add(Request.QueryString);

            if(Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                var parsKeys = pars.AllKeys;
                //foreach (var key in Request.Form.AllKeys)
                foreach (string key in Request.Form.Keys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(key, Request.Unvalidated[key]);
                    }
                }
            }

            var res = WorkflowInit.Runtime.DesignerAPI(pars, out bool hasError, filestream, true);
            var operation = pars["operation"].ToLower();
            if (operation == "downloadscheme" && !hasError)
                return File(Encoding.UTF8.GetBytes(res), "text/xml");
            else if (operation == "downloadschemebpmn" && !hasError)
                return File(UTF8Encoding.UTF8.GetBytes(res), "text/xml");

            return Content(res);
        }

     }
}
