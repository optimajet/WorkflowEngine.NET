using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Sample.Redis.Entities
{
    public class DocumentTransitionHistory
    {
        public Guid Id { get; set; }
        public string AllowedToEmployeeNames;
        public string DestinationState;
        public string InitialState;
        public string Command;
        public Guid? EmployeeId;
        public string EmployeeName;
        public DateTime? TransitionTime;
    }
}
