using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Workflow.Runtime;

namespace Common.WF
{
    public class WorkflowSync : IDisposable
    {
        public WorkflowSync(WorkflowRuntime runtime, Guid workflowInstanceId)
        {
            if (runtime == null) throw new ArgumentNullException("runtime cannot be null", "runtime");
            if (workflowInstanceId == Guid.Empty) throw new ArgumentOutOfRangeException("workflowInstanceId cannot be empty", "workflowInstanceId");

            this.IsDisposed = false;

            this.Runtime = runtime;
            this.WorkflowInstanceId = workflowInstanceId;

            this.Runtime.WorkflowIdled += Runtime_WorkflowIdled;
            this.Runtime.WorkflowCompleted += Runtime_WorkflowIdled; // Runtime_WorkflowCompleted;
            this.Runtime.WorkflowTerminated += Runtime_WorkflowIdled; // Runtime_WorkflowTerminated;

            this.WaitHandle = new AutoResetEvent(false);
        }

        public event EventHandler WorkflowIdled;

        public AutoResetEvent WaitHandle { get; private set; }

        protected void RaiseWorkflowIdled()
        {
            this.WaitHandle.Set();

            EventHandler temp = this.WorkflowIdled;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        void Runtime_WorkflowIdled(object sender, WorkflowEventArgs e)
        {
            if (e.WorkflowInstance.InstanceId == this.WorkflowInstanceId)
                this.RaiseWorkflowIdled();
        }

        Guid WorkflowInstanceId { get; set; }
        WorkflowRuntime Runtime { get; set; }

        #region IDisposable Members

        bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.Runtime.WorkflowIdled -= Runtime_WorkflowIdled;
                this.Runtime.WorkflowCompleted -= Runtime_WorkflowIdled;
                this.Runtime.WorkflowTerminated -= Runtime_WorkflowIdled;

                this.IsDisposed = true;
            }
        }

        ~WorkflowSync()
        {
            this.Dispose();
        }

        #endregion
    }
}
