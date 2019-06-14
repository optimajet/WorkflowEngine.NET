using System;
using System.Reflection;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;
using System.Threading.Tasks;

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
                        var provider = DataServiceProvider.Get<IPersistenceProviderContainer>().Provider;

                        var builder = new WorkflowBuilder<XElement>(provider, new XmlWorkflowParser(), provider).WithDefaultCache();

                        _runtime = new WorkflowRuntime()
                            .WithBuilder(builder)
                            .WithActionProvider(new ActionProvider(DataServiceProvider))
                            .WithRuleProvider(new RuleProvider(DataServiceProvider))
                            .WithPersistenceProvider(provider)
                            .WithTimerManager(new TimerManager())
                            .WithBus(new NullBus())
                            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                            .RegisterAssemblyForCodeActions(Assembly.GetExecutingAssembly())
                            .Start();

                        _runtime.ProcessStatusChanged += _runtime_ProcessStatusChanged;
                    }
                }
            }
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
