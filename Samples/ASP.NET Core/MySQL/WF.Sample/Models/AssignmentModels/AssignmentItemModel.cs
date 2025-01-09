using System;
using System.Collections.Generic;

namespace WF.Sample.Models
{
    public class AssignmentItemModel
    {
        public Guid AssignmentId { get; set; } 
        
        public string AssignmentCode { get; set; }

        public Guid ProcessId { get; set; }
        
        public int? DocumentNumber { get; set; }

        public string Name { get; set; }
        
        public DateTime DateCreation { get; set; }

        public string Executor { get; set; }
        
        public string ExecutorName { get; set; }

        public string StatusState { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
    }
}
