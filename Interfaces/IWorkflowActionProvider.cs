using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    public interface IWorkflowActionProvider
    {
        void ExecuteAction(string name, ProcessInstance processInstance, string actionParameter);
        bool ExecuteCondition(string name, ProcessInstance processInstance, string actionParameter);
        List<string> GetActions();
    }
}
