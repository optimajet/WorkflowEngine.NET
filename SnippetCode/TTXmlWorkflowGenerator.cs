using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace OptimaJet.Workflow.Core.Generator
{
    /// <summary>
    /// Generate process scheme from TT template
    /// </summary>
    public class TTXmlWorkflowGenerator : IWorkflowGenerator<XElement>
    {
        protected IDictionary<string,Type> TemplateTypeMapping = new Dictionary<string, Type>();

        /// <summary>
        /// Generate not parsed process scheme
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="parameters">Parameters for creating scheme</param>
        /// <returns>Not parsed process scheme</returns>
        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            var processTemplateType = TemplateTypeMapping[schemeCode.ToLower()];
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

        /// <summary>
        /// Adds relationship between code of the scheme and TT template type
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="generatorSource">TT template type</param>
        public void AddMapping(string schemeCode, object generatorSource)
        {
            var type = generatorSource as Type;
            if (type == null)
                throw new InvalidOperationException();
            TemplateTypeMapping.Add(schemeCode.ToLower(), type);
        }
    }

   
}
