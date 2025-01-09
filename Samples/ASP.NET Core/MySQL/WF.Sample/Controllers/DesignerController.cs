using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OptimaJet.Workflow;
using WF.Sample.Business.Workflow;


namespace WF.Sample.Controllers
{
    public class DesignerController : Controller
    {
        public Task<ActionResult> Index(string schemeName)
        {
            return Task.FromResult<ActionResult>(View());
        }
        
        public async Task<ActionResult> API()
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
                foreach (string key in Request.Form.Keys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(key, Request.Form[key]);
                    }
                }
            }

            (string res, bool hasError) = await WorkflowInit.Runtime.DesignerAPIAsync(pars, filestream);
            
            var operation = pars["operation"].ToLower();
            if (operation == "downloadscheme" && !hasError)
                return File(Encoding.UTF8.GetBytes(res), "text/xml");

            return Content(res);
        }

     }
}
