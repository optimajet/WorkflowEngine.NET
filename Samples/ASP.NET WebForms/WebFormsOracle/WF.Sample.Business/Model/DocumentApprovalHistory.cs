using System;

namespace WF.Sample.Business.Model
{
    public class DocumentApprovalHistory
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }
        public string AllowedTo { get; set; }
        public DateTime? TransitionTime { get; set; }
        public long Sort { get; set; }
        public string InitialState { get; set; }
        public string DestinationState { get; set; }
        public string TriggerName { get; set; }
        public string Commentary { get; set; }
        public Employee Employee { get; set; }
    }
}
