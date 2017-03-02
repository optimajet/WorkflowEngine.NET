using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Xml.Linq;
using Apache.Ignite.Core;
using OptimaJet.Workflow.Ignite;

namespace WorkflowApp
{
    public class WorkflowInit
    {
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
                            Ignition.ClientMode = true;
                            var store = Ignition.TryGetIgnite() ?? Ignition.Start(IgniteProvider.GetDefaultIgniteConfiguration());

                            var provider = new IgniteProvider(store);

                            var builder = new WorkflowBuilder<XElement>(
                                provider,
                                new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(),
                                provider
                                ).WithDefaultCache();

                            _runtime = new WorkflowRuntime()
                                .WithBuilder(builder)
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
