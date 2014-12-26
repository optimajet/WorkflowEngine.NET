using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Runtime
{
    public interface IWorkflowRuleProvider
    {
        List<string> GetRules();      
        bool Check(Guid processId, string identityId, string ruleName, string parameter);
        IEnumerable<string> GetIdentities(Guid processId, string ruleName, string parameter);
    }
}
