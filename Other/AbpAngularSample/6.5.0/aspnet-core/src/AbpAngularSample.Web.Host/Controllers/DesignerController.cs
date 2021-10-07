using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using AbpAngularSample.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OptimaJet.Workflow;
using OptimaJet.Workflow.Core.Runtime;

namespace AbpAngularSample.Web.Host.Controllers
{
    //WorkflowEngineSampleCode
    public class DesignerController : AbpAngularSampleControllerBase
    {
        private readonly WorkflowRuntime _runtime;
        
        public DesignerController(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult API()
        {
            Stream filestream = null;
            
            var isPost = Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
            if (isPost && Request.Form.Files != null && Request.Form.Files.Count > 0)
                filestream = Request.Form.Files[0].OpenReadStream();

            var pars = new NameValueCollection();
            foreach (var q in Request.Query)
            {
                pars.Add(q.Key, q.Value.First());
            }


            if (isPost)
            {
                var parsKeys = pars.AllKeys;
                //foreach (var key in Request.Form.AllKeys)
                foreach (var key in Request.Form.Keys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(key, Request.Form[key]);
                    }
                }
            }

            var res = _runtime.DesignerAPI(pars, filestream);

            if (pars["operation"].ToLower() == "downloadscheme")
                return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.xml");
            if (pars["operation"].ToLower() == "downloadschemebpmn")
                return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.bpmn");

            return Content(res);
        }
    }
}