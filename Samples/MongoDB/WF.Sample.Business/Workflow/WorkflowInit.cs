using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using MongoDB.Driver;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.MongoDB;
using WF.Sample.Business.Models;

namespace WF.Sample.Business.Workflow
{
    public static class WorkflowInit
    {
        private static IWorkflowBuilder GetDefaultBuilder(MongoDBProvider provider)
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
                            var mongoClient = new MongoClient(new MongoUrl(ConfigurationManager.AppSettings["Url"]));
                            var provider =
                                new MongoDBProvider(mongoClient.GetDatabase(ConfigurationManager.AppSettings["Database"]));
                            var builder = GetDefaultBuilder(provider).WithDefaultCache();

                            _runtime = new WorkflowRuntime()
                                .WithBuilder(builder)
                                .WithActionProvider(new WorkflowActions())
                                .WithRuleProvider(new WorkflowRule())
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

                return _runtime;
            }
        }

        public static MongoDBProvider Provider
        {
            get
            {
                return Runtime.PersistenceProvider as MongoDBProvider;
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
            var dbcoll = Provider.Store.GetCollection<WorkflowInbox>("WorkflowInbox");

            dbcoll.DeleteMany(c => c.ProcessId == e.ProcessId);

            if (e.NewStatus != ProcessStatus.Finalized)
            {
                var d = new Action<ProcessStatusChangedEventArgs>(PreExecuteAndFillInbox);
                d.BeginInvoke(e, FillInboxCallback, null);
            }

            //Change state name
            var docdbcoll = Provider.Store.GetCollection<Document>("Document");
            var document = docdbcoll.Find( x => x.Id == e.ProcessId).FirstOrDefault();
            if (document != null)
            {
                var nextState = Runtime.GetLocalizedStateName(e.ProcessId, e.ProcessInstance.CurrentState);
                document.StateName = nextState;
                docdbcoll.ReplaceOne(x => x.Id == document.Id, document, new UpdateOptions { IsUpsert = true });
            }
        }

        #region Inbox
        private static void FillInboxCallback(IAsyncResult ar)
        {
        }

        private static void PreExecuteAndFillInbox(ProcessStatusChangedEventArgs e)
        {
            var processId = e.ProcessId;

            FillInbox(processId);
        }

        private static void FillInbox(Guid processId)
        {
            var newActors = Runtime.GetAllActorsForDirectCommandTransitions(processId);
            var items = new List<WorkflowInbox>();
            foreach (var newActor in newActors)
            {
                items.Add(new WorkflowInbox() { Id = Guid.NewGuid(), IdentityId = newActor, ProcessId = processId });
            }

            if (items.Any())
            {
                var dbcoll = Provider.Store.GetCollection<WorkflowInbox>("WorkflowInbox");
                dbcoll.InsertMany(items);
            }
        }


        public static void RecalcInbox()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
