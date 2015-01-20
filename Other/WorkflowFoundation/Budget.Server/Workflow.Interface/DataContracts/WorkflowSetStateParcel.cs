using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.DAL.DataContracts;

namespace Budget2.Server.Workflow.Interface.DataContracts
{
    public class WorkflowSetStateParcel
    {
        public string Comment { get; set; }
        public Guid InitiatorId { get; set; }
        public WorkflowCommand Command { get; set; }
        public WorkflowState PreviousWorkflowState { get; set; }
    }
}
