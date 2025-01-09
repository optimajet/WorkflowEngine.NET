using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business.Model;

namespace WF.Sample.Business.Workflow
{
    public class ExternalParametersProvider : IWorkflowExternalParametersProvider
    {
        
        public ExternalParametersProvider()
        {
            
            GetParameter.Add(nameof(Document.Author), (name, pr)=> GetDocument(pr).Author);
            GetParameter.Add(nameof(Document.Manager), (name, pr)=> GetDocument(pr).Manager);
            GetParameter.Add(nameof(Document.Name), (name, pr)=> GetDocument(pr).Name);
            GetParameter.Add(nameof(Document.Number), (name, pr)=> GetDocument(pr).Number);
            GetParameter.Add(nameof(Document.State), (name, pr)=> GetDocument(pr).State);
            GetParameter.Add(nameof(Document.Sum), (name, pr)=> GetDocument(pr).Sum);
            GetParameter.Add(nameof(Document.AuthorId), (name, pr)=> GetDocument(pr).AuthorId);
            GetParameter.Add(nameof(Document.ManagerId), (name, pr)=> GetDocument(pr).ManagerId);
            GetParameter.Add(nameof(Document.StateName), (name, pr)=> GetDocument(pr).StateName);
            GetParameter.Add(nameof(Document), (name, pr)=> GetDocument(pr));
        }
        
        
        public Dictionary<string, Func<string, ProcessInstance, object>> GetParameter = new Dictionary<string, Func<string, ProcessInstance, object>>();

        public Dictionary<string, Func<string, ProcessInstance, Task<object>>> GetParametersAsync = new Dictionary<string, Func<string, ProcessInstance, Task<object>>>();
        
        public Dictionary<string, Action<string, object, ProcessInstance>> SetParameter = new Dictionary<string, Action<string, object, ProcessInstance>>();
        
        public Dictionary<string, Func<string, object, ProcessInstance, Task>> SetParametersAsync = new Dictionary<string, Func<string, object, ProcessInstance, Task>>();

        public Func<ProcessInstance, Document> GetDocument;
        public Task<object> GetExternalParameterAsync(string parameterName, ProcessInstance processInstance)
        {
            return GetParametersAsync[parameterName](parameterName, processInstance);
        }

        public object GetExternalParameter(string parameterName, ProcessInstance processInstance)
        {
            return GetParameter[parameterName](parameterName, processInstance);
        }

        public Task SetExternalParameterAsync(string parameterName, object parameterValue, ProcessInstance processInstance)
        {
            return SetParametersAsync[parameterName](parameterName, parameterValue, processInstance);
        }

        public void SetExternalParameter(string parameterName, object parameterValue, ProcessInstance processInstance)
        {
            SetParameter[parameterName](parameterName, parameterValue, processInstance);
        }

        public bool IsGetExternalParameterAsync(string parameterName, string schemeCode, ProcessInstance processInstance)
        {
            return GetParametersAsync.ContainsKey(parameterName);
        }

        public bool IsSetExternalParameterAsync(string parameterName, string schemeCode, ProcessInstance processInstance)
        {
            return SetParametersAsync.ContainsKey(parameterName);
        }

        public bool HasExternalParameter(string parameterName, string schemeCode, ProcessInstance processInstance)
        {
            return GetParameter.ContainsKey(parameterName) || SetParameter.ContainsKey(parameterName) 
                                                           || GetParametersAsync.ContainsKey(parameterName) 
                                                           || SetParametersAsync.ContainsKey(parameterName);
        }
    }
}
