using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OptimaJet.Workflow.Core.Persistence;

namespace WF.Sample.Models
{
    public class OutboxDocumentModel:DocumentModel
    {
        [Display(Name = "First approval time")]
        public DateTime? FirstApprovalTime { get; set; }
        [Display(Name = "Last approval time")]
        public DateTime? LastApprovalTime { get; set; }
        [Display(Name = "Approval count")]
        public int ApprovalCount { get; set; }
        [Display(Name = "Last Approval")]
        public string LastApproval { get; set; }
    }
}
