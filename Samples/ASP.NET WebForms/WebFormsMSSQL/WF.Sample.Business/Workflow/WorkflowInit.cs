using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;
using System.Threading.Tasks;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace WF.Sample.Business.Workflow
{
    public static class WorkflowInit
    {
        private static volatile WorkflowRuntime _runtime;

        private static readonly object _sync = new object();

        public static IDataServiceProvider DataServiceProvider { get; private set; }

        public static WorkflowRuntime Create(IDataServiceProvider dataServiceProvider)
        {
            DataServiceProvider = dataServiceProvider;
            CreateRuntime();
            return Runtime;
        }

        private static void CreateRuntime()
        {
            if (_runtime == null)
            {
                lock (_sync)
                {
                    if (_runtime == null)
                    {
                        var plugin = new OptimaJet.Workflow.Core.Plugins.BasicPlugin();
                        //Settings for SendEmail actions
                        // plugin.Setting_Mailserver = "smtp.yourserver.com";
                        // plugin.Setting_MailserverPort = 25;
                        // plugin.Setting_MailserverFrom = "from@yourserver.com";
                        // plugin.Setting_MailserverLogin = "login@yourserver.com";
                        // plugin.Setting_MailserverPassword = "password";
                        // plugin.Setting_MailserverSsl = true;
                        plugin.UsersInRoleAsync = UsersInRoleAsync;
                        
                        var provider = DataServiceProvider.Get<IPersistenceProviderContainer>().Provider;

                        var builder = new WorkflowBuilder<XElement>(provider, new XmlWorkflowParser(), provider).WithDefaultCache();

                        _runtime = new WorkflowRuntime()
                            .WithBuilder(builder)
                            .WithActionProvider(new ActionProvider(DataServiceProvider))
                            .WithRuleProvider(new RuleProvider(DataServiceProvider))
                            .WithPersistenceProvider(provider)
                            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                            .RegisterAssemblyForCodeActions(Assembly.GetExecutingAssembly())
                            .WithPlugin(plugin)
                            .AsSingleServer() //.AsMultiServer()
                            .Start();
                       
                        _runtime.ProcessStatusChanged += _runtime_ProcessStatusChanged;
                    }
                }
            }
        }

        public static async Task<IEnumerable<string>> UsersInRoleAsync(string roleName, ProcessInstance processInstance)
        {
            var provider = DataServiceProvider.Get<IEmployeeRepository>();
            return provider.GetInRole(roleName);            
        }

        public static WorkflowRuntime Runtime => _runtime;

        static void _runtime_ProcessStatusChanged(object sender, ProcessStatusChangedEventArgs e)
        {
            if (e.NewStatus != ProcessStatus.Idled && e.NewStatus != ProcessStatus.Finalized)
                return;

            if (string.IsNullOrEmpty(e.SchemeCode))
                return;

            DataServiceProvider.Get<IDocumentRepository>().DeleteEmptyPreHistory(e.ProcessId);
            _runtime.PreExecuteFromCurrentActivity(e.ProcessId);

            //Inbox
            var ir = DataServiceProvider.Get<IInboxRepository>();
            ir.DropWorkflowInbox(e.ProcessId);

            if (e.NewStatus != ProcessStatus.Finalized)
            {
                Task.Run(() => PreExecuteAndFillInbox(e));
            }

            //Change state name
            if (!e.IsSubprocess)
            {
                var nextState = e.ProcessInstance.CurrentState;
                if(nextState == null)
                {
                    nextState = e.ProcessInstance.CurrentActivityName;
                }
                var nextStateName = Runtime.GetLocalizedStateName(e.ProcessId, nextState);

                var docRepository = DataServiceProvider.Get<IDocumentRepository>();

                docRepository.ChangeState(e.ProcessId, nextState, nextStateName);
            }
        }

        #region Inbox
        private static void PreExecuteAndFillInbox(ProcessStatusChangedEventArgs e)
        {
            DataServiceProvider.Get<IInboxRepository>().FillInbox(e.ProcessId, Runtime);
        }

        public static void RecalcInbox()
        {
            DataServiceProvider.Get<IInboxRepository>().RecalcInbox(Runtime);
        }
        #endregion
    }
}
