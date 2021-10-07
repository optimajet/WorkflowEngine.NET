using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.DbPersistence;

namespace AbpAngularSample.Workflow
{
    //WorkflowEngineSampleCode
    public static class WorkflowRuntimeManager
    {
        private static WorkflowRuntime Runtime { get; set; }

        public static WorkflowRuntime InitWorkflowRuntime(IServiceProvider provider, IConfigurationRoot config)
        {
            var connectionString =  config.GetConnectionString("Default");
            
            var dbProvider = new MSSQLProvider(connectionString);

            var builder = new WorkflowBuilder<XElement>(dbProvider, new XmlWorkflowParser(), dbProvider).WithDefaultCache();

            Runtime = new WorkflowRuntime()
                .WithBuilder(builder)
                .WithPersistenceProvider(dbProvider)
                .WithActionProvider(provider.GetService<IWorkflowActionProvider>())
                .EnableCodeActions()
                .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn();
            
            Runtime.ProcessStatusChanged += Runtime_ProcessStatusChanged;
            
            Runtime.Start();
            
            return Runtime;
        }

        public delegate Task ChangeState(object id, string state, string localizedState);
        public static readonly ConcurrentDictionary<string, ChangeState> ChangeStateFuncs = new();


        private static void Runtime_ProcessStatusChanged(object sender, ProcessStatusChangedEventArgs e)
        {
            if (e.NewStatus != ProcessStatus.Idled && e.NewStatus != ProcessStatus.Finalized)
                return;

            if (string.IsNullOrEmpty(e.SchemeCode))
                return;

            Runtime.PreExecuteFromCurrentActivity(e.ProcessId);

            //Change state name
            if (!e.IsSubprocess)
            {
                var nextState = e.ProcessInstance.CurrentState;
                var nextStateName = Runtime.GetLocalizedStateName(e.ProcessId, e.ProcessInstance.CurrentState);

                var objectId = e.ProcessInstance.GetParameter<int>("objectId");
                var objectType = e.ProcessInstance.GetParameter<string>("objectType");

                if (objectType != null && objectId != null)
                {
                    if (ChangeStateFuncs.ContainsKey(objectType))
                    {
                        ChangeStateFuncs[objectType].Invoke(objectId, nextState, nextStateName).Wait();
                    }
                    else
                    {
                        throw new Exception(string.Format("{0} type is not supported", objectType));
                    }
                }
            }
        }
    }
}