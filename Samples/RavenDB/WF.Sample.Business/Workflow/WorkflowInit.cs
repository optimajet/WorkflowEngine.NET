using System;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.RavenDB;
using Raven.Client;
using Raven.Client.Document;
using WF.Sample.Business.Models;

namespace WF.Sample.Business.Workflow
{
    public static class WorkflowInit
    {
        private static IWorkflowBuilder GetDefaultBuilder(RavenDBProvider provider)
        {
            var builder = new WorkflowBuilder<XElement>(provider,
                new XmlWorkflowParser(),
                provider
                ).WithDefaultCache();
            return builder;
        }

        private static volatile WorkflowRuntime _runtime;

        private static readonly object _sync = new object();

        public static WorkflowRuntime Runtime
        {
            get
            {
                if (_runtime == null)
                {
                    lock (_sync)
                    {
                        if (_runtime == null)
                        {
                            var provider = new RavenDBProvider(new DocumentStore()
                            {
                                Url = ConfigurationManager.AppSettings["Url"],
                                DefaultDatabase = ConfigurationManager.AppSettings["Database"]
                            });
                            var builder = GetDefaultBuilder(provider).WithDefaultCache();

                            _runtime = new WorkflowRuntime(new Guid("{8D38DB8F-F3D5-4F26-A989-4FDD40F32D9D}"))
                                .WithBuilder(builder)
                                .WithActionProvider(new WorkflowActions())
                                .WithRuleProvider(new WorkflowRule())
                                .WithPersistenceProvider(provider)
                                .WithTimerManager(new TimerManager())
                                .WithBus(new NullBus())
                                .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                                .Start();

                            _runtime.ProcessStatusChanged += new EventHandler<ProcessStatusChangedEventArgs>(_runtime_ProcessStatusChanged);
                        }
                    }
                }

                return _runtime;
            }
        }

        public static RavenDBProvider Provider
        {
            get
            {
                return Runtime.PersistenceProvider as RavenDBProvider;
            }
        }

        static void _runtime_ProcessStatusChanged(object sender, ProcessStatusChangedEventArgs e)
        {
            if (e.NewStatus != ProcessStatus.Idled && e.NewStatus != ProcessStatus.Finalized)
                return;

            if (string.IsNullOrEmpty(e.SchemeCode))
                return;

            WorkflowActions.DeleteEmptyPreHistory(e.ProcessId);
            _runtime.PreExecuteFromCurrentActivity(e.ProcessId);

            //Inbox
            using (var session = Provider.Store.OpenSession())
            {
                var inboxDocs = session.Query<WorkflowInbox>().Where(c => c.ProcessId == e.ProcessId).ToList();
                foreach (var d in inboxDocs)
                    session.Delete(d);
                session.SaveChanges();
            }

            if (e.NewStatus != ProcessStatus.Finalized)
            {
                var d = new Action<ProcessStatusChangedEventArgs>(PreExecuteAndFillInbox);
                d.BeginInvoke(e, FillInboxCallback, null);
            }

            //Change state name
            var nextState = Runtime.GetLocalizedStateName(e.ProcessId, e.ProcessInstance.CurrentState);
            using (var session = Provider.Store.OpenSession())
            {
                var document = session.Load<Document>(e.ProcessId);
                if (document != null)
                {
                    document.StateName = nextState;
                    session.SaveChanges();
                }
            }
        }

        #region Inbox
        private static void FillInboxCallback(IAsyncResult ar)
        {
        }

        private static void PreExecuteAndFillInbox(ProcessStatusChangedEventArgs e)
        {
            var processId = e.ProcessId;

            using (var session = Provider.Store.OpenSession())
            {
                FillInbox(processId, session);
                session.SaveChanges();
            }
        }

        private static void FillInbox(Guid processId, IDocumentSession session)
        {
            var newActors = Runtime.GetAllActorsForDirectCommandTransitions(processId);
            foreach (var newActor in newActors)
            {
                var newInboxItem = new WorkflowInbox() { IdentityId = newActor, ProcessId = processId };
                session.Store(newInboxItem);
            }
        }


        public static void RecalcInbox()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
