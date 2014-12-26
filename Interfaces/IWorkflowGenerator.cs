using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Generator
{
    public interface IWorkflowGenerator<out TSchemeMedium> where TSchemeMedium : class
    {
        TSchemeMedium Generate(string SchemeCode, Guid schemeId, IDictionary<string, object> parameters);
    }
}
