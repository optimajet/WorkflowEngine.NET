using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Data.Linq;
using System.Linq;
using OptimaJet.Workflow.Core.Generator;

namespace OptimaJet.Workflow.DbPersistence
{
    public class DbXmlWorkflowGenerator : DbProvider, IWorkflowGenerator<XElement>
    {
        protected IDictionary<string, string> TemplateTypeMapping = new Dictionary<string, string>();

        public DbXmlWorkflowGenerator(string connectionStringName) : base(connectionStringName)
        {
        }

        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count > 0)
                throw new InvalidOperationException("Parameters not supported");

            var code = !TemplateTypeMapping.ContainsKey(schemeCode.ToLower()) ? schemeCode : TemplateTypeMapping[schemeCode.ToLower()];
            WorkflowScheme scheme = null;
            using (var context = CreateContext())
            {
                scheme = context.WorkflowSchemes.FirstOrDefault(ws => ws.Code == code);
            }

            if (scheme == null)
                throw new InvalidOperationException(string.Format("Scheme with Code={0} not found",code));

            return XElement.Parse(scheme.Scheme);
        }

    }
}
