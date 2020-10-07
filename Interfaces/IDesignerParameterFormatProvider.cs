using OptimaJet.Workflow.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Interface for parameter format provider (only for the Designer)
    /// </summary>
    public interface IDesignerParameterFormatProvider
    {
        /// <summary>
        /// Returns parameter definitions for a specified code action
        /// </summary>
        /// <param name="type">Code action type <see cref="CodeActionType"/></param>
        /// <param name="name">Name of the code action</param>
        /// <returns>A list of <see cref="CodeActionParameterDefinition"/></returns>
        List<CodeActionParameterDefinition> GetFormat(CodeActionType type, string name, string schemeCode);
    }
}
