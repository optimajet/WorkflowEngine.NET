using System;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.MongoDB.Models
{
    public class WorkflowRuntime : DynamicEntity
    {
        public string RuntimeId { get; set; }
        public Guid Lock { get; set; }
        public RuntimeStatus Status { get; set; }
        public string RestorerId { get; set; }
        public DateTime? NextTimerTime { get; set; }
        public DateTime? NextServiceTimerTime { get; set; }
        public DateTime? LastAliveSignal { get; set; }
    }
}
