using System;
using System.Collections.Generic;

namespace WF.Sample.Models
{
    public class AssignmentInfoModel
    {
        public Guid? AssignmentId { get; set; } 
        
        public string AssignmentCode { get; set; }

        public Guid ProcessId { get; set; }
        
        public int? DocumentNumber { get; set; }

        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? DateCreation { get; set; }
        
        public DateTime? DateStart { get; set; }
        
        public DateTime? DateFinish { get; set; }
        
        public DateTime? DeadlineToStart { get; set; }
        
        public DateTime? DeadlineToComplete { get; set; }

        public Guid? Executor { get; set; }
        
        public string ExecutorName { get; set; }
        
        public Dictionary<Guid, string> Observers { get; set; }

        public List<string> Tags { get; set; }
        
        public string StatusState { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public List<string> Statuses { get; set; } = new List<string>();
        public Dictionary<Guid, string> Employees { get; set; } = new Dictionary<Guid, string>();
        public string FormAction { get; set; } = "Update"; //or "Create"
    }
}
