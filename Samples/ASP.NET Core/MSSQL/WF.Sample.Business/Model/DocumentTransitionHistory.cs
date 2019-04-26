using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.Business.Model
{
    public class DocumentTransitionHistory
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public string AllowedToEmployeeNames { get; set; }
        public DateTime? TransitionTime { get; set; }
        public long Order { get; set; }
        public string InitialState { get; set; }
        public string DestinationState { get; set; }
        public string Command { get; set; }
      
    }
}
