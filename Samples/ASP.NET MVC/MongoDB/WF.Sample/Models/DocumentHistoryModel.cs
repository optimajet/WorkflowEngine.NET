using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.Model;

namespace WF.Sample.Models
{
    public class DocumentHistoryModel
    {
        public List<DocumentApprovalHistory> Items { get; set; }
    }
}
