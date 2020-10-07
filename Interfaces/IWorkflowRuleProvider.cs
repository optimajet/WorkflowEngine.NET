using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        List<string> GetRules(string schemeCode);

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
        /// Check the rule asynchronously
        /// </summary>
        /// <param name="processInstance">Reference to ProcessInstance for which rule is checked <see cref="ProcessInstance"/></param>
        /// <param name="runtime">Reference to WorkflowRuntime object which managed specified process instance <see cref="WorkflowRuntime"/></param>
        /// <param name="identityId">User id for which rule is checking </param>
        /// <param name="ruleName">Name of the rule to check</param>
        /// <param name="parameter">Additional rule parameter <see cref="RestrictionDefinition.Actor"/> <see cref="ActorDefinition.Value"/></param>
        /// <returns>Rule check result</returns>
        Task<bool> CheckAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName, string parameter, CancellationToken token);


        /// <summary>
        /// Get the list of users which satisfies the rule
        /// </summary>
        /// <param name="processInstance">Reference to ProcessInstance for which rule is checked <see cref="ProcessInstance"/></param>
        /// <param name="runtime">Reference to WorkflowRuntime object which managed specified process instance <see cref="WorkflowRuntime"/></param>
        /// <param name="ruleName">Name of the rule to get users list</param>
        /// <param name="parameter">Additional rule parameter <see cref="RestrictionDefinition.Actor"/> <see cref="ActorDefinition.Value"/></param>
        /// <returns>List of users which satisfies the rule</returns>
        IEnumerable<string> GetIdentities(ProcessInstance processInstance, WorkflowRuntime runtime, string ruleName, string parameter);

        /// <summary>
        /// Get the list of users which satisfies the rule asynchronously
        /// </summary>
        /// <param name="processInstance">Reference to ProcessInstance for which rule is checked <see cref="ProcessInstance"/></param>
        /// <param name="runtime">Reference to WorkflowRuntime object which managed specified process instance <see cref="WorkflowRuntime"/></param>
        /// <param name="ruleName">Name of the rule to get users list</param>
        /// <param name="parameter">Additional rule parameter <see cref="RestrictionDefinition.Actor"/> <see cref="ActorDefinition.Value"/></param>
        /// <returns>List of users which satisfies the rule</returns>
        Task<IEnumerable<string>> GetIdentitiesAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string ruleName, string parameter, CancellationToken token);

        /// <summary>
        /// Checks whether Check the rule should be called asynchronously
        /// </summary>
        /// <param name="ruleName">Name of the action</param>
        bool IsCheckAsync(string ruleName, string schemeCode);

        /// <summary>
        /// Checks whether Get the list of users which satisfies the rule should be called asynchronously
        /// </summary>
        /// <param name="ruleName">Name of the condition</param>
        bool IsGetIdentitiesAsync(string ruleName, string schemeCode);
    }
}
