using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;
using System.Xml.Linq;

namespace OptimaJet.EasyWorkflow.Core
{
    public static class Converter
    {
        public static ProcessDefinition ToProcessDefinition(string schemeCode, List<WorkflowBlock> blocks)
        {
            var pd = new ProcessDefinition { Name = schemeCode };

            foreach (var block in blocks)
            {
                block.Register(pd,blocks);
            }

            foreach (var block in blocks)
            {
                block.RegisterFinal(pd, blocks);
            }

            return pd;
        }

        public static string ToString(string schemeCode, List<WorkflowBlock> blocks)
        {
            var pd = ToProcessDefinition(schemeCode, blocks);
            return pd.Serialize();
        }

        public static XElement ToXElement(string schemeCode, List<WorkflowBlock> blocks)
        {
            return XElement.Parse(ToString(schemeCode, blocks));
        }
    }
}
