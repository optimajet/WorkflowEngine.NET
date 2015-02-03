using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core;

namespace OptimaJet.Workflow.DbPersistence
{
    public sealed class DbSchemePersistenceProvider : DbProvider,ISchemePersistenceProvider<XElement>
    {
        public DbSchemePersistenceProvider(string connectionStringName) : base(connectionStringName)
        {
        }

        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            WorkflowProcessInstance processInstance;
            using (var context = CreateContext())
            {
                processInstance = context.WorkflowProcessInstances.FirstOrDefault(pis => pis.Id == processId);
            }

           if (processInstance == null)
               throw new ProcessNotFoundException();

            if (!processInstance.SchemeId.HasValue)
                throw new SchemeNotFoundException();
            var schemeDefinition = GetProcessSchemeBySchemeId(processInstance.SchemeId.Value);
            schemeDefinition.IsDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;
            return schemeDefinition;
        }

        public SchemeDefinition<XElement> GetProcessSchemeBySchemeId(Guid schemeId)
        {
            WorkflowProcessScheme processScheme;
            using (var context = CreateContext())
            {
                processScheme = context.WorkflowProcessSchemes.FirstOrDefault(pss => pss.Id == schemeId);
            }

            if (processScheme == null || string.IsNullOrEmpty(processScheme.Scheme))
                throw new SchemeNotFoundException();

            return new SchemeDefinition<XElement>(schemeId, processScheme.SchemeCode, XElement.Parse(processScheme.Scheme), processScheme.IsObsolete, processScheme.DefiningParameters);
        }

        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, IDictionary<string, object> parameters)
        {
            return GetProcessSchemeWithParameters(schemeCode, parameters, false);
        }

        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, IDictionary<string,object> parameters, bool ignoreObsolete)
        { 
            IEnumerable<WorkflowProcessScheme> processSchemes;
            var definingParameters = SerializeParameters(parameters);
            var hash = HashHelper.GenerateStringHash(definingParameters);
            
            using (var context = CreateContext())
            {
                processSchemes = context.WorkflowProcessSchemes.Where(pss => pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash && (!ignoreObsolete || !pss.IsObsolete)).ToList();
            }

            if (processSchemes.Count() < 1)
                throw new SchemeNotFoundException();
            
            if (processSchemes.Count() == 1)
            {
                var scheme = processSchemes.First();
                return new SchemeDefinition<XElement>(scheme.Id, schemeCode, XElement.Parse(scheme.Scheme), scheme.IsObsolete);
            }

            foreach (var processScheme in processSchemes.Where(processScheme => processScheme.DefiningParameters == definingParameters))
            {
                return new SchemeDefinition<XElement>(processScheme.Id, schemeCode, XElement.Parse(processScheme.Scheme), processScheme.IsObsolete);
            }

            throw new SchemeNotFoundException();
        }

        public void SetSchemeIsObsolete(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = SerializeParameters(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (var scope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var oldSchemes =
                        context.WorkflowProcessSchemes.Where(
                            wps =>
                                wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == schemeCode &&
                                !wps.IsObsolete).ToList();

                    foreach (var scheme in oldSchemes)
                    {
                        scheme.IsObsolete = true;
                    }

                    context.SubmitChanges();
                }
            }
        }

        public void SetSchemeIsObsolete(string schemeCode)
        {
            using (var scope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var oldSchemes =
                        context.WorkflowProcessSchemes.Where(
                            wps =>
                                wps.SchemeCode == schemeCode && !wps.IsObsolete).ToList();

                    foreach (var scheme in oldSchemes)
                    {
                        scheme.IsObsolete = true;
                    }

                    context.SubmitChanges();
                }
            }
        }

        public void SaveScheme(string schemeCode, Guid schemeId, XElement scheme, IDictionary<string, object> parameters)
        {
            var definingParameters = SerializeParameters(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (var scope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var oldSchemes =
                        context.WorkflowProcessSchemes.Where(
                            wps => wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == schemeCode && !wps.IsObsolete).ToList();

                    if (oldSchemes.Count() > 0)
                    {
                        foreach (var oldScheme in oldSchemes)
                            if (oldScheme.DefiningParameters == definingParameters)
                                throw new SchemeAlredyExistsException();
                    }


                    var newProcessScheme = new WorkflowProcessScheme
                                               {
                                                   Id = schemeId,
                                                   DefiningParameters = definingParameters,
                                                   DefiningParametersHash = definingParametersHash,
                                                   Scheme = scheme.ToString(),
                                                   SchemeCode = schemeCode
                                               };
                    context.WorkflowProcessSchemes.InsertOnSubmit(newProcessScheme);
                    context.SubmitChanges();
                }

                scope.Complete();
            }

        }

        private string SerializeParameters (IDictionary<string, object> parameters)
        {
            var json = new StringBuilder("{");

            bool isFirst = true;

            foreach (var parameter in parameters.OrderBy(p=>p.Key))
            {
                if (string.IsNullOrEmpty(parameter.Key))
                    continue;

                if (!isFirst)
                    json.Append(",");

                json.AppendFormat("{0}:[",parameter.Key);

                var isSubFirst = true;

                if (parameter.Value is IEnumerable)
                {
                    var enumerableValue = (parameter.Value as IEnumerable);

                    var valuesToString = new List<string>();

                    foreach (var val in enumerableValue)
                    {
                        valuesToString.Add(val.ToString());
                    }

                    foreach (var parameterValue in valuesToString.OrderBy(p => p))
                    {
                        if (!isSubFirst)
                            json.Append(",");
                        json.AppendFormat("\"{0}\"", parameterValue);
                        isSubFirst = false;
                    }
                }
                else
                {
                    json.AppendFormat("\"{0}\"", parameter.Value);
                }

                json.Append("]");

                isFirst = false;

            }

            json.Append("}");

            return json.ToString();
        }

        public XElement GetScheme(string code)
        {
            WorkflowScheme wfScheme;
            using (var context = CreateContext())
            {
                wfScheme = context.WorkflowSchemes.FirstOrDefault(ws => ws.Code == code);
            }
            
            if (wfScheme == null || string.IsNullOrEmpty(wfScheme.Scheme))
                throw new SchemeNotFoundException();

            return XElement.Parse(wfScheme.Scheme);
        }


        public void SaveScheme(string code, string scheme)
        {
            WorkflowScheme wfScheme;
            using (var context = CreateContext())
            {
                wfScheme = context.WorkflowSchemes.FirstOrDefault(ws => ws.Code == code);
                if(wfScheme == null)
                {
                    wfScheme = new WorkflowScheme();
                    wfScheme.Code = code;
                    wfScheme.Scheme = scheme;
                    context.WorkflowSchemes.InsertOnSubmit(wfScheme);
                }
                else
                {
                    wfScheme.Scheme = scheme;
                }
                context.SubmitChanges();
            }
        }
    }
}
