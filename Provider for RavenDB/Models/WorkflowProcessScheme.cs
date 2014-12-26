using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.RavenDB
{
    public class WorkflowProcessScheme
    {
        public string DefiningParameters { get; set; }
        public string DefiningParametersHash { get; set; }
        public Guid Id { get; set; }
        public bool IsObsolete { get; set; }
        public string SchemeCode { get; set; }
        public string Scheme { get; set; }
    }
}
