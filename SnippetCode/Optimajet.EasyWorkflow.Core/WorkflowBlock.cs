using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.EasyWorkflow.Core
{
    public abstract class WorkflowBlock
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> Parameters { get; private set; }
        
        public WorkflowBlock(string name, string type, Dictionary<string, object> parameters)
        {
            Name = name;
            Type = type;
            Parameters = parameters;
        }

        public object this[string parameterName]
        {
            get
            {
                return Parameters.ContainsKey(parameterName) ? Parameters[parameterName] : null;
            }
            set
            {
                if (Parameters.ContainsKey(parameterName))
                    Parameters[parameterName] = value;
                else
                    Parameters.Add(parameterName, value);
            }
        }
   
        public T TryCast<T>(string parameterName, T defaultValue)
        {
            if (this[parameterName] != null && this[parameterName] is T)
                return (T)this[parameterName];
            return defaultValue;
        }

        public virtual void Register(ProcessDefinition pd, List<WorkflowBlock> blocks)
        {
            
        }

        public virtual void RegisterFinal(ProcessDefinition pd, List<WorkflowBlock> blocks)
        {

        }

        public virtual bool Validate(Dictionary<string, object> parameters, out string message)
        {
            message = string.Empty;
            return true;
        }
    }
}
