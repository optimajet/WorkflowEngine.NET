using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Generator
{
    /// <summary>
    /// Interface of a workflow generator, which generates not parsed process scheme <see cref="SchemeDefinition{T}"/> 
    /// </summary>
    /// <typeparam name="TSchemeMedium">Type of not parsed scheme</typeparam>
    public interface IWorkflowGenerator<TSchemeMedium> where TSchemeMedium : class
    {
        /// <summary>
        /// Generate not parsed process scheme
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="parameters">Parameters for creating scheme</param>
        /// <returns>Not parsed process scheme</returns>
        Task<TSchemeMedium> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters);
    }
}
