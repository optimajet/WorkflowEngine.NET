using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Workflow.Activities;

namespace Common.WF
{
    public class StateMachineWithSimpleContainer : StateMachineWorkflowActivity
    {
        public Dictionary<string , object > WorkflowPersistanceParameters = new Dictionary<string , object >();

        public const string DontWriteToWorkflowHistoryPersistenceKey = "{A6DAF70A-56FE-4331-A194-0CAEBC9A2AF2}";

        protected bool DontWriteToWorkflowHistory
        {
            get
            {
                if (WorkflowPersistanceParameters.ContainsKey(DontWriteToWorkflowHistoryPersistenceKey))
                    return (bool)WorkflowPersistanceParameters[DontWriteToWorkflowHistoryPersistenceKey];
                return false;
            }
            set { WorkflowPersistanceParameters[DontWriteToWorkflowHistoryPersistenceKey] = value; }
        }

        protected  void ClearPersistanceParameters ()
        {
            WorkflowPersistanceParameters.Clear();
        }

        protected override System.Workflow.ComponentModel.ActivityExecutionStatus HandleFault(System.Workflow.ComponentModel.ActivityExecutionContext executionContext, Exception exception)
        {
            Logger.Log.Error(string.Format("Ошибка маршрута Id={0} ({1})",WorkflowInstanceId,exception.Message));
            return base.HandleFault(executionContext, exception);
        }
    }
}
