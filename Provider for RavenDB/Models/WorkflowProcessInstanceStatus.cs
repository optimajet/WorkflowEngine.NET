using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.RavenDB
{
    public class WorkflowProcessInstanceStatus
    {
        public Guid Id { get; set; }
        public Guid Lock { get; set; }
        public byte Status { get; set; }
    }
}
