using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Interface of a rule provider, which provide rule checking and getting of a list of users which satisfies a rule <see cref="TransitionDefinition.Restrictions"/>
    /// </summary>
    public interface IWorkflowRuleProvider
    {
        /// <summary>
        /// Return all rule names
        /// </summary>
        /// <returns>List of rule names</returns>
        List<string> GetRules();

        /// <summary>
        /// Check the rule
        /// </summary>
        /// <param name="processInstance">Reference to ProcessInstance for which rule is checked <see cref="ProcessInstance"/></param>
        /// <param name="runtime">Reference to WorkflowRuntime object which managed specified process instance <see cref="WorkflowRuntime"/></param>
        /// <param name="identityId">User id for which rule is checking </param>
        /// <param name="ruleName">Name of the rule to check</param>
        /// <param name="parameter">Additional rule parameter <see cref="RestrictionDefinition.Actor"/> <see cref="ActorDefinition.Value"/></param>
        /// <returns>Rule check result</returns>
        bool Check(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName, string parameter);

        /// <summary>
        /// Get the list of users which satisfies the rule
        /// </summary>
        /// <param name="processInstance">Reference to ProcessInstance for which rule is checked <see cref="ProcessInstance"/></param>
        /// <param name="runtime">Reference to WorkflowRuntime object which managed specified process instance <see cref="WorkflowRuntime"/></param>
        /// <param name="ruleName">Name of the rule to get users list</param>
        /// <param name="parameter">Additional rule parameter <see cref="RestrictionDefinition.Actor"/> <see cref="ActorDefinition.Value"/></param>
        /// <returns>List of users which satisfies the rule</returns>
        IEnumerable<string> GetIdentities(ProcessInstance processInstance, WorkflowRuntime runtime, string ruleName, string parameter);
    }
}
