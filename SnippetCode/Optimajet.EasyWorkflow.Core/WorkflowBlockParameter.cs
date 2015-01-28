using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.EasyWorkflow.Core
{
    public class WorkflowBlockParameter
    {
        public WorkflowBlockParameter(string name, string visibleName, bool required = false)
        {
            Name = name;
            VisibleName = visibleName;
            Required = required;
        }

        public string Name { get; set; }

        public string VisibleName { get; set; }

        public bool Required { get; set; }
    }
}
