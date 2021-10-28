using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Redis
{
    public class ProcessInstanceTreeItem : IProcessInstanceTreeItem
    {
        private ProcessInstanceTreeItem()
        {}
        public Guid Id { get; set; }
        public Guid? ParentProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public string SubprocessName { get; set; }
        public string StartingTransition { get; set; }

        public static List<IProcessInstanceTreeItem> Create(Guid rootProcessId, List<(Guid processId,Guid schemeId, Guid? parentProcessId, Guid rootProcessId, string subprocessName)>
            instances, Dictionary<Guid,string> startingTransitions)
        {
            var res = new List<IProcessInstanceTreeItem> {new ProcessInstanceTreeItem() {Id = rootProcessId, RootProcessId = rootProcessId}};
           
            foreach ((Guid processId, Guid schemeId, Guid? parentProcessId, Guid rootProcessId, string subprocessName) instance in instances)
            {
                res.Add(new ProcessInstanceTreeItem()
                {
                    Id = instance.processId,
                    ParentProcessId = instance.parentProcessId,
                    RootProcessId = instance.rootProcessId,
                    SubprocessName = instance.subprocessName,
                    StartingTransition = startingTransitions[instance.schemeId]
                });
            }

            return res;
        }
    }
}
