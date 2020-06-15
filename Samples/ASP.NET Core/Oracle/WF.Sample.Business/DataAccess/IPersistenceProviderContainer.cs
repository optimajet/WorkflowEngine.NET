using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WF.Sample.Business.DataAccess
{
    public interface IPersistenceProviderContainer
    {
        IWorkflowProvider Provider { get; }
    }
}
