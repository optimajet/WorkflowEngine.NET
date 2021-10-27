using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WF.Sample.Models
{
    public class AssignmentInfoModel
    {
        public Guid? AssignmentId { get; set; } 
        
        [Required]
        public string AssignmentCode { get; set; }

        public Guid ProcessId { get; set; }
        
        public int? DocumentNumber { get; set; }

        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? DateCreation { get; set; }
        
        public DateTime? DateStart { get; set; }
        
        public DateTime? DateFinish { get; set; }
        
        public string DeadlineToStart { get; set; }

        public string DeadlineToComplete { get; set; }

        [Required]
        public Guid? Executor { get; set; }
        
        public string ExecutorName { get; set; }

        public Dictionary<Guid, string> Observers { get; set; } = new Dictionary<Guid, string>();

        public List<string> Tags { get; set; } = new List<string>();
        
        public string StatusState { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public string FormAction { get; set; } = "Update"; //or "Create"
    }
}
