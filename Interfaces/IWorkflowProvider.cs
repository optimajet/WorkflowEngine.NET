using OptimaJet.Workflow.Core.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OptimaJet.Workflow.Core.Persistence
{
    /// <summary>
    /// An interface of a workflow provider wich combines <see cref="IPersistenceProvider"/>, 
    /// <see cref="ISchemePersistenceProvider{XElement}"/>, and <see cref="IWorkflowGenerator{XElement}"/>
    /// </summary>
    public interface IWorkflowProvider : IPersistenceProvider, ISchemePersistenceProvider<XElement>, IWorkflowGenerator<XElement>
    {
    }
}
