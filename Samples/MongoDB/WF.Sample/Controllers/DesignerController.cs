using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WF.Sample.Models;
using System.Configuration;
using WF.Sample.Business.Workflow;
using OptimaJet.Workflow.Core.Builder;
using System.Xml.Linq;
using WF.Sample.Business;
using System.Text;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow;
using System.IO;
using System.Collections.Specialized;

namespace WF.Sample.Controllers
{
    public class DesignerController : Controller
    {
        public ActionResult Index(string schemeName)
        {
            var model = new DesignerModel()
            {
                SchemeName = schemeName
            };

            return View(model);
        }
        
        public ActionResult API()
        {
            Stream filestream = null;
            if (Request.Files.Count > 0)
                filestream = Request.Files[0].InputStream;

            var pars = new NameValueCollection();
            pars.Add(Request.Params);
            
            if(Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                var parsKeys = pars.AllKeys;
                foreach (var key in Request.Form.AllKeys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(Request.Form);
                    }
                }
            }

            var res = WorkflowInit.Runtime.DesignerAPI(pars, filestream, true);
            if (pars["operation"].ToLower() == "downloadscheme")
                return File(UTF8Encoding.UTF8.GetBytes(res), "text/xml", "scheme.xml");
            return Content(res);
        }
    }
}
