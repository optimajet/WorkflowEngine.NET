using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
    public interface ITimerManager
    {
        void RegisterTimers(ProcessInstance processInstance);

        void ClearAndRegisterTimers(ProcessInstance processInstance);

        void ClearTimers(ProcessInstance processInstance);

        void Init(WorkflowRuntime runtime);

        void Start();

        void Stop();

        void Refresh();
    }
}
