using System;
using System.Collections.Generic;
using System.Text;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.MongoDB;

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
