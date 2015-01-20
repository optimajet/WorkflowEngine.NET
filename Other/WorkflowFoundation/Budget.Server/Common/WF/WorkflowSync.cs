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
        public bool WasTerminated { get; private set; }
        public bool WasIdled { get; private set; }
        public bool WasCompleted { get; private set; }

        public WorkflowSync(WorkflowRuntime runtime, Guid workflowInstanceId)
        {
            if (runtime == null) throw new ArgumentNullException("runtime cannot be null", "runtime");
            if (workflowInstanceId == Guid.Empty) throw new ArgumentOutOfRangeException("workflowInstanceId cannot be empty", "workflowInstanceId");

            this.IsDisposed = false;

            this.Runtime = runtime;
            this.WorkflowInstanceId = workflowInstanceId;

            this.Runtime.WorkflowIdled += Runtime_WorkflowIdled;
            this.Runtime.WorkflowCompleted += Runtime_WorkflowCompleted; // Runtime_WorkflowCompleted;
            this.Runtime.WorkflowTerminated += Runtime_WorkflowTerminated; // Runtime_WorkflowTerminated;

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
            WasIdled = true;
            if (e.WorkflowInstance.InstanceId == this.WorkflowInstanceId)
                this.RaiseWorkflowIdled();
        }

        void Runtime_WorkflowCompleted(object sender, WorkflowEventArgs e)
        {
            WasCompleted = true;
            if (e.WorkflowInstance.InstanceId == this.WorkflowInstanceId)
                this.RaiseWorkflowIdled();
        }

        void Runtime_WorkflowTerminated(object sender, WorkflowEventArgs e)
        {
            WasTerminated = true;
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
                this.Runtime.WorkflowCompleted -= Runtime_WorkflowCompleted;
                this.Runtime.WorkflowTerminated -= Runtime_WorkflowTerminated;

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
