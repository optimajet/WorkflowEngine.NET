using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Cache;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Builder
{
    /// <summary>
    /// Interface of a workflow builder, which convert not parsed process scheme <see cref="SchemeDefinition{T}"/> to the object model of a scheme of a process <see cref="ProcessDefinition"/>
    /// </summary>
    public interface IWorkflowBuilder
    {
        /// <summary>
        /// Create new instance of the process.
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        /// <returns>ProcessInstance object</returns>
        ProcessInstance CreateNewProcess(Guid processId,
                                         string schemeCode,
                                         IDictionary<string, object> parameters);


        ProcessInstance CreateNewSubprocess(Guid processId,
            ProcessInstance parentProcessInstance,
            TransitionDefinition startingTransition
            );

       
        /// <summary>
        /// Create new scheme for existing process
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        /// <returns>ProcessDefinition object</returns>
        ProcessDefinition CreateNewProcessScheme(string schemeCode, IDictionary<string, object> parameters);

        ProcessDefinition CreateNewSubprocessScheme(ProcessDefinition parentProcessScheme,
            TransitionDefinition startingTransition);

        /// <summary>
        /// Returns existing process instance
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>ProcessInstance object</returns>
        ProcessInstance GetProcessInstance(Guid processId);

        /// <summary>
        /// Returns process scheme by specific id, if scheme not exists creates it 
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        ProcessDefinition GetProcessScheme(Guid schemeId);

        /// <summary>
        /// Sets the cache to store parsed ProcessDefinition objects <see cref="ProcessDefinition"/> 
        /// </summary>
        /// <param name="cache">Instance of cache object</param>
        void SetCache(IParsedProcessCache cache);

        /// <summary>
        /// Removes the cache to store parsed ProcessDefinition objects <see cref="ProcessDefinition"/> 
        /// </summary>
        void RemoveCache();

        /// <summary>
        /// Returns process scheme by specific name, if scheme not exists creates it
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        ProcessDefinition GetProcessScheme(string schemeCode);

        /// <summary>
        /// Returns process scheme by specific name and parameters for creating the scheme of the process, if scheme not exists creates it
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        /// <returns>ProcessDefinition object</returns>
        ProcessDefinition GetProcessScheme(string schemeCode, IDictionary<string, object> parameters);

        /// <summary>
        /// Set IsObsolete sign to the scheme with specific name and parameters for creating the scheme of the process
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        void SetSchemeIsObsolete(string schemeCode, Dictionary<string, object> parameters);

        /// <summary>
        /// Set IsObsolete sign to the scheme with specific name
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        void SetSchemeIsObsolete(string schemeCode);
        
        /// <summary>
        /// Returns existing process scheme directly from scheme persistence store
        /// </summary>
        /// <param name="code">Name of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        ProcessDefinition GetProcessSchemeForDesigner(string code);

        /// <summary>
        /// Saves process scheme to scheme persistence store
        /// </summary>
        /// <param name="schemecode">Code of the scheme</param>
        /// <param name="pd">Object representation of the scheme</param>
        void SaveProcessScheme(string schemecode, ProcessDefinition pd);

        /// <summary>
        /// Parses process scheme from the string
        /// </summary>
        /// <param name="scheme">String representation of not parsed scheme</param>
        /// <returns>ProcessDefinition object</returns>
        ProcessDefinition Parse(string scheme);

        /// <summary>
        /// Serialize process scheme to the string
        /// </summary>
        /// <param name="processDefinition">SProcessDefinition object</param>
        /// <returns>String representation of not parsed scheme</returns>
        string Serialize(ProcessDefinition processDefinition);
    }

}
