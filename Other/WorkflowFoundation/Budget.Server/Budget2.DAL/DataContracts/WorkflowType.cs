using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.DAL.DataContracts
{
    [Serializable]
    public class WorkflowType
    {
        public Guid Id { get; set; }

        public IEnumerable<string> StatesToIgnoreInTracking { get; private set; }

        public static readonly WorkflowType BillDemandWorkfow = new WorkflowType() { Id = new Guid("DBA4C29A-E6CC-445C-A132-681BD2184FA1"), StatesToIgnoreInTracking = new List<string> { "Archived" } };

        public static readonly WorkflowType DemandAdjustmentWorkflow = new WorkflowType() { Id = new Guid("382ACCF8-EC54-43FC-A463-ACB83DA906CE"), StatesToIgnoreInTracking = new List<string> { "Archived" } };

        public static readonly WorkflowType DemandWorkflow = new WorkflowType() { Id = new Guid("91D95AB4-FC49-49E0-AFC9-C08834C3154B"), StatesToIgnoreInTracking = new List<string> { "Archived" } };
    }
}
