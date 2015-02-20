using OptimaJet.Workflow;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Runtime;
using System.Collections.Generic;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using WorkflowRuntime = OptimaJet.Workflow.Core.Runtime.WorkflowRuntime;

namespace WF.Sample.Controllers
{
    public class RuleProvider : IWorkflowRuleProvider
    {

        public bool Check(Guid processId, string identityId, string ruleName, string parameter)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<string> GetIdentities(Guid processId, string ruleName, string parameter)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.List<string> GetRules()
        {
            //LIST YOUR RULES NAMES HERE
            return new List<string>() { "" };
        }
    }

    public class ActionProvider : IWorkflowActionProvider
    {

        public void ExecuteAction(string name, OptimaJet.Workflow.Core.Model.ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            throw new NotImplementedException();
        }

        public bool ExecuteCondition(string name, OptimaJet.Workflow.Core.Model.ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            throw new NotImplementedException();
        }

        public List<string> GetActions()
        {
            //LIST YOUR ACTIONS NAMES HERE
            return new List<string>()
            {
                ""
            };
        }
    }

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
            pars.Add(Request.Params);

            if (Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
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

            var res = getRuntime.DesignerAPI(pars, filestream, true);
            if (pars["operation"].ToLower() == "downloadscheme")
                return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.xml");
            return Content(res);
        }

        private static volatile WorkflowRuntime _runtime;
        private static readonly object _sync = new object();
        private WorkflowRuntime getRuntime
        {
            get
            {
                if (_runtime == null)
                {
                    lock (_sync)
                    {
                        if (_runtime == null)
                        {
                            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                            var provider = new OptimaJet.Workflow.DbPersistence.OracleProvider(connectionString);
                            var builder = new WorkflowBuilder<XElement>(
                                provider,
                                new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(),
                                provider
                                ).WithDefaultCache();

                            _runtime = new WorkflowRuntime(new Guid("{8D38DB8F-F3D5-4F26-A989-4FDD40F32D9D}"))
                                .WithBuilder(builder)
                                .WithActionProvider(new ActionProvider())
                                .WithRuleProvider(new RuleProvider())
                                .WithPersistenceProvider(provider)
                                .WithTimerManager(new TimerManager())
                                .WithBus(new NullBus())
                                .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                                .Start();
                        }
                    }
                }

                return _runtime;
            }
        }
    }
}


 