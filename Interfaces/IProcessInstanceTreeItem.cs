using System;

namespace OptimaJet.Workflow.Core.Persistence
{
    public interface IProcessInstanceTreeItem
    {
        Guid Id { get; set; }
        Guid? ParentProcessId { get; set; }
        Guid RootProcessId { get; set; }
        string SubprocessName { get; set; }
        string StartingTransition { get; set; }
    }
}
