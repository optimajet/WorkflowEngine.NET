using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using ServiceStack.Text;

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

            return ConvertToSchemeDefinition(processScheme);
        }

        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string schemeCode, string definingParameters,
            Guid? rootSchemeId, bool ignoreObsolete)
        {
            IEnumerable<WorkflowProcessScheme> processSchemes;
            var hash = HashHelper.GenerateStringHash(definingParameters);

            using (var context = CreateContext())
            {
                processSchemes =
                    context.WorkflowProcessSchemes.Where(
                        pss =>
                            pss.SchemeCode == schemeCode && pss.DefiningParametersHash == hash &&
                            (!ignoreObsolete || !pss.IsObsolete) && ((!rootSchemeId.HasValue && pss.RootSchemeId == null) ||  pss.RootSchemeId == rootSchemeId)).ToList();
            }

            if (!processSchemes.Any())
                throw new SchemeNotFoundException();

            if (processSchemes.Count() == 1)
            {
                var scheme = processSchemes.First();
                return ConvertToSchemeDefinition(scheme);
            }

            foreach (
                var processScheme in
                    processSchemes.Where(processScheme => processScheme.DefiningParameters == definingParameters))
            {
                return ConvertToSchemeDefinition(processScheme);
            }

            throw new SchemeNotFoundException();
        }

        public void SetSchemeIsObsolete(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = DefiningParametersSerializer.Serialize(parameters);
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (var scope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var oldSchemes =
                        context.WorkflowProcessSchemes.Where(
                            wps =>
                                wps.DefiningParametersHash == definingParametersHash && (wps.SchemeCode == schemeCode || wps.RootSchemeCode == schemeCode) &&
                                !wps.IsObsolete).ToList();

                    foreach (var scheme in oldSchemes)
                    {
                        scheme.IsObsolete = true;
                    }

                    context.SubmitChanges();
                }

                scope.Complete();
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
                                (wps.SchemeCode == schemeCode || wps.RootSchemeCode == schemeCode) && !wps.IsObsolete).ToList();

                    foreach (var scheme in oldSchemes)
                    {
                        scheme.IsObsolete = true;
                    }

                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        public void SaveScheme(SchemeDefinition<XElement> scheme)
        {
            var definingParameters = scheme.DefiningParameters;
            var definingParametersHash = HashHelper.GenerateStringHash(definingParameters);

            using (var scope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (var context = CreateContext())
                {
                    var oldSchemes =
                        context.WorkflowProcessSchemes.Where(
                            wps => wps.DefiningParametersHash == definingParametersHash && wps.SchemeCode == scheme.SchemeCode && wps.IsObsolete == scheme.IsObsolete).ToList();

                    if (oldSchemes.Any())
                    {
                        if (oldSchemes.Any(oldScheme => oldScheme.DefiningParameters == definingParameters))
                        {
                            throw new SchemeAlredyExistsException();
                        }
                    }


                    var newProcessScheme = new WorkflowProcessScheme
                                               {
                                                   Id = scheme.Id,
                                                   DefiningParameters = definingParameters,
                                                   DefiningParametersHash = definingParametersHash,
                                                   Scheme = scheme.Scheme.ToString(),
                                                   SchemeCode = scheme.SchemeCode,
                                                   RootSchemeCode =  scheme.RootSchemeCode,
                                                   RootSchemeId = scheme.RootSchemeId,
                                                   AllowedActivities = JsonSerializer.SerializeToString(scheme.AllowedActivities),
                                                   StartingTransition = scheme.StartingTransition
,                                               };
                    context.WorkflowProcessSchemes.InsertOnSubmit(newProcessScheme);
                    context.SubmitChanges();
                }

                scope.Complete();
            }

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

        private SchemeDefinition<XElement> ConvertToSchemeDefinition(WorkflowProcessScheme workflowProcessScheme)
        {
            return new SchemeDefinition<XElement>(workflowProcessScheme.Id, workflowProcessScheme.RootSchemeId,
                workflowProcessScheme.SchemeCode, workflowProcessScheme.RootSchemeCode,
                XElement.Parse(workflowProcessScheme.Scheme), workflowProcessScheme.IsObsolete, false,
                JsonSerializer.DeserializeFromString<List<string>>(workflowProcessScheme.AllowedActivities), workflowProcessScheme.StartingTransition,
                workflowProcessScheme.DefiningParameters); 
        }
    }
}
