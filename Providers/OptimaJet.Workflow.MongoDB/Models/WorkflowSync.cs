using System;
using System.Collections.Generic;
using System.Text;

namespace OptimaJet.Workflow.MongoDB.Models
{
    public class WorkflowSync : DynamicEntity
    {
        public string Name { get; set; }
        public Guid Lock { get; set; }
    }
}
