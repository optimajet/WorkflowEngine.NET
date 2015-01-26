using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace OptimaJet.Workflow.Core.Generator
{
    public class TTXmlWorkflowGenerator : IWorkflowGenerator<XElement>
    {
        protected IDictionary<string,Type> TemplateTypeMapping = new Dictionary<string, Type>(); 

        public XElement Generate(string SchemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            var processTemplateType = TemplateTypeMapping[SchemeCode.ToLower()];
            var sessionProperty = processTemplateType.GetProperty("Session", typeof(IDictionary<string, object>));
            var transformTextMethod = processTemplateType.GetMethod("TransformText");
            var initializeMethod = processTemplateType.GetMethod("Initialize");
            var obj = Activator.CreateInstance(processTemplateType, false);

            var session = (IDictionary<string, object>)sessionProperty.GetGetMethod(false).Invoke(obj,new object[]{});

            if (session == null)
            {
                session = new Dictionary<string, object>();
                sessionProperty.GetSetMethod(false).Invoke(obj, new object[] {session});
            }

            session.Clear();

            foreach (var parameter in parameters)
                session.Add(parameter.Key,parameter.Value);

            session.Add("SchemeId",schemeId);

            initializeMethod.Invoke(obj, new object[] {});

            var output = (string) transformTextMethod.Invoke(obj, new object[] {});

            return XElement.Parse(output);
        }

        public void AddMapping(string SchemeCode, object generatorSource)
        {
            var type = generatorSource as Type;
            if (type == null)
                throw new InvalidOperationException();
            TemplateTypeMapping.Add(SchemeCode.ToLower(), type);
        }
    }

   
}
