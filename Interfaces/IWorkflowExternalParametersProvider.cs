using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    public interface IWorkflowExternalParametersProvider
    {
        Task<object> GetExternalParameterAsync(string parameterName, ProcessInstance processInstance);
        object GetExternalParameter(string parameterName, ProcessInstance processInstance);
        Task SetExternalParameterAsync(string parameterName, object parameterValue, ProcessInstance processInstance);
        void SetExternalParameter(string parameterName, object parameterValue, ProcessInstance processInstance);
        bool IsGetExternalParameterAsync(string parameterName, string schemeCode);
        bool IsSetExternalParameterAsync(string parameterName, string schemeCode);
        bool HasExternalParameter(string parameterName, string schemeCode);
    }

    /// <summary>
    /// Empty external parameters provider
    /// </summary>
    public class EmptyWorkflowExternalParametersProvider : IWorkflowExternalParametersProvider
    {
        public Task<object> GetExternalParameterAsync(string parameterName, ProcessInstance processInstance)
        {
            throw new System.NotImplementedException();
        }

        public object GetExternalParameter(string parameterName, ProcessInstance processInstance)
        {
            throw new System.NotImplementedException();
        }

        public Task SetExternalParameterAsync(string parameterName, object parameterValue, ProcessInstance processInstance)
        {
            throw new System.NotImplementedException();
        }

        public void SetExternalParameter(string parameterName, object parameterValue, ProcessInstance processInstance)
        {
            throw new System.NotImplementedException();
        }

        public bool IsGetExternalParameterAsync(string parameterName, string schemeCode)
        {
            return false;
        }

        public bool IsSetExternalParameterAsync(string parameterName, string schemeCode)
        {
            return false;
        }

        public bool HasExternalParameter(string parameterName, string schemeCode)
        {
            return false;
        }
    }
}
