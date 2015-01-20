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
        protected Dictionary<string , object > WorkflowPersistanceParameters = new Dictionary<string , object >();

        protected void ClearPersistanceParameters ()
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
